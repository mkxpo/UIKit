using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Core {

    /// <summary>
    /// Базовый тип для редактируемы хобъектов
    /// </summary>
    public abstract class DataObjectBase : IComparable, INotifyPropertyChanged {

        [NotMapped]
        private Dictionary<string, object> ShadowCopy;

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName]string propertyName = null) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [NotMapped]
        public virtual string StringRepresentation {
            get {
                return ToString();
            }
        }

        [NotMapped]
        public bool IsEditing { get; private set; } = false;

        public virtual void BeginEdit() {
            ShadowCopy = new Dictionary<string, object>();
            var properties = this.GetType().GetProperties();
            foreach (var property in properties) {
                ShadowCopy[property.Name] = property.GetValue(this);
            }
            IsEditing = true;
        }


        public virtual void CancelEdit() {
            IsEditing = false;
            var properties = this.GetType().GetProperties();
            foreach (var property in properties) {
                if (property.CanWrite) {
                    property.SetValue(this, ShadowCopy[property.Name]);
                }
            }
        }

        public virtual void InitNewObject(DbContext dataContext) {
        }


        public virtual void BeforeAdd(DbContext dataContext) {
        }


        public virtual void BeforeUpdate(DbContext dataContext) {
        }


        public virtual void BeforeDelete(DbContext dataContext) {
        }

        public virtual void OnPropertyChanged(DbContext dataContext, string propertyName) {
        }


        public virtual string Validate(DbContext dataContext) {
            PropertyInfo[] properties = ReflectionHelper.GetVisibleProperties(this);
            foreach (PropertyInfo property in properties) {
                object value = property.GetValue(this);
                string name = ReflectionHelper.GetPropertyName(property);
                if (ReflectionHelper.IsPropertyRequired(property)) {
                    if (value == null
                        || (value is string && string.IsNullOrWhiteSpace(value.ToString()))) {
                        return string.Format("Необходимо заполнить поле '{0}'.", name);
                    }
                    if (value != null && value.GetType().IsValueType) {
                        Type type = value.GetType();
                        object defaultValue = Activator.CreateInstance(type);
                        if (value.Equals(defaultValue)) {
                            return string.Format("Необходимо заполнить поле '{0}'.", name);
                        }
                    }
                    if (value != null && value is IEnumerable && !((IEnumerable)value).Cast<object>().Any()) {
                        return string.Format("Необходимо заполнить список '{0}'.", name);
                    }
                }
                int maxLength = ReflectionHelper.GetPropertyTextMaxLength(property);
                if (maxLength > 0) {
                    if (value != null && value.ToString().Length > maxLength) {
                        return string.Format("Текст в поле '{0}' имеет слишком большую длину ({1}) при максимально допустимой {2}",
                            name, value.ToString().Length, maxLength);
                    }
                }
            }
            return "";
        }

        public int CompareTo(object obj) {
            if (obj == null) return 1;
            return string.Compare(this.ToString(), obj.ToString());
        }

        public override bool Equals(object obj) {
            DataObjectBase other = obj as DataObjectBase;
            if (other == null) {
                return false;
            }
            return ReflectionHelper.GetObjectID(this) == ReflectionHelper.GetObjectID(other);
        }

        public override int GetHashCode() {
            return ReflectionHelper.GetObjectID(this);
        }

        public override string ToString() {
            return ReflectionHelper.GetDefaultStringRepresentation(this);
        }
    }
}
