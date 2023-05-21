using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Core.Windows;
using System.Drawing;

namespace Core {

    /// <summary>
    /// Вспомогательный класс для отображения окон редакторов объектов
    /// </summary>
    public static class UIHelper {

        public static TModel CreateObject<TModel>(Control ownerWindow, DbContext dbContext = null)
            where TModel : DataObjectBase {
            return (TModel)CreateObject(typeof(TModel), ownerWindow, dbContext, true);
        }

        public static DataObjectBase CreateObject(Type dataObjectType, Control ownerWindow, DbContext dbContext = null, bool autoSave = false, Func<object, bool> afterObjectCreated = null) {
            Cursor.Current = Cursors.WaitCursor;
            DbContext dataContext = (dbContext != null) ? dbContext : ServiceProvider.DataContextFactory.CreateDbContext();
            try {
                if (SecurityHelper.IsCreateAllowed(dataObjectType) == false) {
                    throw new UnauthorizedAccessException(string.Format("Недостаточно прав доступа для создания объекта '{0}'.", ReflectionHelper.GetTypeName(dataObjectType)));
                }
                DataObjectBase model = (DataObjectBase)Activator.CreateInstance(dataObjectType);
                model.InitNewObject(dataContext);
                if (afterObjectCreated != null) {
                    if (!afterObjectCreated(model)) {
                        return null;
                    }
                }
                Form dlg = CreateEditorWindow(model, dataContext);
                ArrangeToParentWindow(dlg, ownerWindow);
                dlg.Closing += (sender, e) => {
                    if (dlg.DialogResult == DialogResult.OK) {
                        try {
                            Cursor.Current = Cursors.WaitCursor;
                            string msg = model.Validate(dataContext);
                            if (!string.IsNullOrEmpty(msg)) {
                                Warning(dlg, msg);
                                e.Cancel = true;
                                return;
                            }
                            model.BeforeAdd(dataContext);
                            dataContext.Add(model);
                            if (autoSave) {
                                dataContext.SaveChanges();
                            }
                        } catch (Exception ex) {
                            Error(dlg, ex);
                            e.Cancel = true;
                        } finally {
                            Cursor.Current = Cursors.Default;
                        }
                    }
                };
                if (dlg.ShowDialog() == DialogResult.OK) {
                    return model;
                } else {
                    return null;
                };
            } catch (Exception ex) {
                Error(null, ex);
                return null;

            } finally {
                if (dbContext == null) {
                    dataContext.Dispose();
                }
                Cursor.Current = Cursors.Default;
            }
        }

        public static TModel EditObject<TModel>(int objectId, Form ownerWindow, DbContext dbContext = null)
            where TModel : DataObjectBase {
            Cursor.Current = Cursors.WaitCursor;
            DbContext dataContext = (dbContext != null) ? dbContext : ServiceProvider.DataContextFactory.CreateDbContext();
            try {
                if (SecurityHelper.IsReadAllowed(typeof(TModel)) == false) {
                    throw new UnauthorizedAccessException(string.Format("Недостаточно прав доступа для просмотра объекта '{0}'.", ReflectionHelper.GetTypeName(typeof(TModel))));
                }
                var model = (TModel)dataContext.Find<TModel>(objectId);
                return (TModel)EditObject(model, ownerWindow, dataContext, true);
            } catch (Exception ex) {
                Error(null, ex);
                return null;
            } finally {
                if (dbContext == null) {
                    dataContext.Dispose();
                }
                Cursor.Current = Cursors.Default;
            }
        }

        public static DataObjectBase EditObject(DataObjectBase model, Control ownerWindow = null, DbContext dbContext = null, bool autoSave = false) {
            Cursor.Current = Cursors.WaitCursor;
            DbContext dataContext = null;
            try {
                if (SecurityHelper.IsReadAllowed(model.GetType()) == false) {
                    throw new UnauthorizedAccessException(string.Format("Недостаточно прав доступа для просмотра объекта '{0}'.", ReflectionHelper.GetTypeName(model.GetType())));
                }
                dataContext = (dbContext != null) ? dbContext : ServiceProvider.DataContextFactory.CreateDbContext();
                model.BeginEdit();
                Form dlg = CreateEditorWindow(model, dataContext);
                if (dlg.StartPosition != FormStartPosition.CenterParent) {
                    ArrangeToParentWindow(dlg, ownerWindow);
                }
                dlg.Closing += (sender, e) => {
                    if (dlg.DialogResult == DialogResult.OK) {
                        try {
                            Cursor.Current = Cursors.WaitCursor;
                            string msg = model.Validate(dataContext);
                            if (!string.IsNullOrEmpty(msg)) {
                                Warning(dlg, msg);
                                e.Cancel = true;
                                return;
                            }
                            model.BeforeUpdate(dataContext);
                            if (autoSave) {
                                dataContext.SaveChanges();
                            }
                        } catch (Exception ex) {
                            Error(dlg, ex);
                            e.Cancel = true;
                        } finally {
                            Cursor.Current = Cursors.Default;
                        }
                    }
                };
                if (dlg.ShowDialog() == DialogResult.OK) {
                    return model;
                } else {
                    model.CancelEdit();
                    return null;
                };
            } catch (Exception ex) {
                Error(null, ex);
                model.CancelEdit();
                return null;
            } finally {
                if (dbContext == null) {
                    dataContext.Dispose();
                }
                if (ownerWindow != null) {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        public static bool EditObjectWithoutDbContext(DataObjectBase model, Form ownerWindow = null) {
            return (EditObject(model, ownerWindow, null, false) != null);
        }

        public static DataObjectBase EditCollection(Type collectionItemType, Form ownerWindow, bool selectionRequired = false) {
            Cursor.Current = Cursors.WaitCursor;
            DbContext dataContext = ServiceProvider.DataContextFactory.CreateDbContext();
            try {
                CollectionEditorWindow dlg = CreateListWindow(collectionItemType, dataContext, selectionRequired);
                ArrangeToParentWindow(dlg, ownerWindow);
                dlg.Closing += (sender, e) => {
                    try {
                        dlg.Cursor = Cursors.WaitCursor;
                    } catch (Exception ex) {
                        Error(dlg, ex);
                        e.Cancel = true;
                    } finally {
                        Cursor.Current = Cursors.Default;
                    }
                };
                if(dlg.ShowDialog() == DialogResult.OK) {
                    if (selectionRequired) {
                        return dlg.SelectedItem as DataObjectBase;
                    } else {
                        return null;
                    }
                }
                return null;
            } catch (Exception ex) {
                Error(null, ex);
                return null;
            } finally {
                dataContext.Dispose();
                Cursor.Current = Cursors.Default;
            }
        }

        public static CollectionEditorControl CreateEditCollectionControl(Type collectionItemType) {
            DbContext dataContext = ServiceProvider.DataContextFactory.CreateDbContext();
            var control = new CollectionEditorControl(collectionItemType, dataContext, false);
            control.Disposed += (s, e) => {
                dataContext.Dispose();
            };
            return control;
        }

        public static bool DeleteObject(DataObjectBase model) {
            DbContext dataContext = ServiceProvider.DataContextFactory.CreateDbContext();
            try {
                return DeleteObject(model, dataContext, true);
            } finally {
                dataContext.Dispose();
            }
        }

        public static bool DeleteObject(DataObjectBase model, DbContext dbContext, bool autoSave) {
            if (model == null) {
                throw new ArgumentNullException(nameof(model));
            }
            if (dbContext == null) {
                throw new ArgumentNullException(nameof(dbContext));
            }
            DbContext dataContext = (dbContext != null) ? dbContext : ServiceProvider.DataContextFactory.CreateDbContext();
            try {
                if (SecurityHelper.IsDeleteAllowed(model.GetType()) == false) {
                    throw new UnauthorizedAccessException(string.Format("Недостаточно прав доступа для удаления объекта '{0}'.", ReflectionHelper.GetTypeName(model.GetType())));
                }
                object objectId = ReflectionHelper.GetObjectID(model);
                Type modelType = EFHelper.GetEntityTypeFromProxy(model);
                DataObjectBase obj = (DataObjectBase)dataContext.Find(modelType, objectId);
                if (obj == null) {
                    return true;
                }
                if (MessageBox.Show(string.Format("Удалить '{0}'?", obj), "Удаление записи", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                    obj.BeforeDelete(dataContext);
                    dataContext.Remove(obj);
                    if (autoSave) {
                        dataContext.SaveChanges();
                    }
                    return true;
                } else {
                    return false;
                }
            } catch (Exception ex) {
                Error(null, ex);
                return false;
            } finally {
                if (dbContext == null) {
                    dataContext.Dispose();
                }
            }
        }

        public static bool Execute(this Form window, Action action) {
            try {
                Cursor.Current = Cursors.WaitCursor;
                action();
                return true;
            } catch(Exception ex) {
                while (ex.InnerException != null) {
                    ex = ex.InnerException;
                }
                Error(window, ex.Message);
                return false;
            } finally {
                Cursor.Current = Cursors.Default;
            }
        }

        public static void Info(this Control window, string title, string text, params object[] parameters) {
            text = string.Format(text, parameters);
            MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Warning(this Control window, string text, params object[] parameters) {
            text = string.Format(text, parameters);
            MessageBox.Show(text, "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void Error(this Control window, Exception ex) {
            while (ex.InnerException != null) {
                ex = ex.InnerException;
            }
            Error(window, "{0}", string.Join("\r\n", ex.Message));
        }

        public static void Error(this Control window, string text, params object[] parameters) {
            text = string.Format(text, parameters);
            MessageBox.Show(text, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool Confirm(this Control window, string text, params object[] parameters) {
            text = string.Format(text, parameters);
            return (MessageBox.Show(text, "Подтверждение операции", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes);
        }

        static void ArrangeToParentWindow(Form window, Control parent) {
            if (parent is Form parentWindow) {
                window.Left = parentWindow.Left + 32;
                window.Top = parent.Top + 32;
            } else {
                window.StartPosition = FormStartPosition.CenterScreen;
            }
        }

        static EditorWindow CreateEditorWindow(DataObjectBase model, DbContext dataContext) {
            int width, height;
            string title;
            bool? centerToParent;
            Type editorFormType = ReflectionHelper.GetEditorFormType(model.GetType(), out width, out height, out title, out centerToParent);
            if (editorFormType == null) {
                editorFormType = typeof(EditorWindow);
            }
            var form = (EditorWindow)Activator.CreateInstance(editorFormType, new object[] { dataContext, model });
            if (width > 0) {
                //form.SizeToContent = (form.SizeToContent | SizeToContent.Width) ^ SizeToContent.Width;
                form.Width = (int)(width * form.DeviceDpi / 96.0);
            }
            if (height > 0) {
                //form.SizeToContent = (form.SizeToContent | SizeToContent.Height) ^ SizeToContent.Height;
                form.Height = (int)(height * form.DeviceDpi / 96.0);
            }
            if (!string.IsNullOrEmpty(title)) {
                form.Text = title;
            }
            if (centerToParent != null && centerToParent.Value) {
                form.StartPosition = FormStartPosition.CenterParent;
            }
            return form;
        }

        static CollectionEditorWindow CreateListWindow(Type collectionItemType, DbContext dataContext, bool selectionRequired = false) {
            int width, height;
            string title;
            Type formType = ReflectionHelper.GetListFormType(collectionItemType, out width, out height, out title);
            if (formType == null) {
                formType = typeof(CollectionEditorWindow);
            }
            var form = (CollectionEditorWindow)Activator.CreateInstance(formType, new object[] { collectionItemType, dataContext, selectionRequired });
            if (width > 0) {
               // form.SizeToContent = (form.SizeToContent | SizeToContent.Width) ^ SizeToContent.Width;
                form.Width = width;
            }
            if (height > 0) {
                //form.SizeToContent = (form.SizeToContent | SizeToContent.Height) ^ SizeToContent.Height;
                form.Height = height;
            }
            if (title != null) {
                form.Text = title;
            }
            form.StartPosition = FormStartPosition.CenterParent;
            return form;
        }

        /// <summary>
        /// Заполнение выпадающего списка элементами.
        /// Элементы упорядочиваются по отображаемому тексту
        /// </summary>
        public static void FillDropDownList(ComboBox comboBox, IEnumerable items, string nullElementText) {
            string oldValue = comboBox.SelectedItem != null ? comboBox.SelectedItem.ToString() : nullElementText;
            comboBox.Items.Clear();
            if (nullElementText != null) {
                comboBox.Items.Add(nullElementText);
            }
            comboBox.Items.AddRange(items.Cast<object>().OrderBy(t => t.ToString()).ToArray());
            if (oldValue != null) {
                comboBox.SelectedItem = comboBox.Items.Cast<object>().FirstOrDefault(t => t.ToString() == oldValue);
            }
        }
    }
}
