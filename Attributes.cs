using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core {

    /// <summary>
    /// Базовый класс атрибута, определяющего списка ролей пользователя
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class AllowAccessAttributeBase : Attribute {
        public string Roles { get; private set; }
        public AllowAccessAttributeBase(string roles) {
            Roles = roles;
        }
    }

    /// <summary>
    /// Атрибут определяющий роли, имеющие доступ на чтение объекта
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class AllowReadAttribute : AllowAccessAttributeBase {
        public AllowReadAttribute(string roles) : base(roles) { }
    }

    /// <summary>
    /// Атрибут определяющий роли, имеющие доступ на создание объекта
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AllowCreateAttribute : AllowAccessAttributeBase {
        public AllowCreateAttribute(string roles) : base(roles) { }
    }

    /// <summary>
    /// Атрибут определяющий роли, имеющие доступ на редактирование объекта
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class AllowEditAttribute : AllowAccessAttributeBase {
        public AllowEditAttribute(string roles) : base(roles) { }
    }

    /// <summary>
    /// Атрибут определяющий роли, имеющие доступ на удаление объекта
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AllowDeleteAttribute : AllowAccessAttributeBase {
        public AllowDeleteAttribute(string roles) : base(roles) { }
    }

    /// <summary>
    /// Атрибут определяющий параметры окна редактирования объекта
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EditorFormAttribute : Attribute {
        public Type FormType { get; set; }
        public string FormTypeName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Title { get; set; }
        public bool CenterToParent { get; set; }
    }

    /// <summary>
    /// Атрибут определяющий параметры окна списка объектов
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ListFormAttribute : Attribute {
        public Type FormType { get; set; }
        public string FormTypeName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Title { get; set; }
    }

    /// <summary>
    /// Атрибут, наличие которого определяет начало новой группы элементов управления на форме
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class BeginNewGroupAttribute : Attribute {
        public string Title { get; private set; }
        public BeginNewGroupAttribute(string title) {
            Title = title;
        }
    }

    /// <summary>
    /// Атрибут определяющий направление сортировки списка объектов по полю
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SortDirectionAttribute : Attribute {
        public ListSortDirection Direction { get; private set; }
        public SortDirectionAttribute(ListSortDirection direction) {
            Direction = direction;
        }
    }

    /// <summary>
    /// Атрибут определяющий видимость поля объекта (в списке, в окне редактора или везде)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class VisibilityAttribute : Attribute {
        public FieldVisibility Visibility { get; private set; }
        public VisibilityAttribute(FieldVisibility visibility) {
            Visibility = visibility;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class TabAttribute : Attribute {
        public string TabTitle { get; private set; }
        public TabAttribute(string title) {
            TabTitle = title;
        }
    }

    public enum FieldVisibility {
        None = 0,
        Form = 1,
        List = 2,
        Both = 3,
    }
}
