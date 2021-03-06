﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using Xappy.Content.ControlGallery;

namespace Xappy.ControlGallery
{
    [QueryProperty("ControlTitle", "control")]
    public class ControlPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetAndRaisePropertyChanged<TRef>(
            ref TRef field, TRef value, [CallerMemberName] string propertyName = null)
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetAndRaisePropertyChangedIfDifferentValues<TRef>(
            ref TRef field, TRef value, [CallerMemberName] string propertyName = null)
            where TRef : class
        {
            if (field == null || !field.Equals(value))
            {
                SetAndRaisePropertyChanged(ref field, value, propertyName);
            }
        }

        public ControlType SelectedControl { get; set; }

        public ICommand ViewXAMLCommand { get; set; }

        public ICommand UndoCommand { get; set; }

        public ICommand RedoCommand { get; set; }

        public ICommand ResetCommand { get; set; }

        public ICommand SelectCommand { get; set; }

        public ICommand SwitchedCommand { get; set; }

        public ObservableCollection<PropertyInfo> Properties { get => properties; set => SetAndRaisePropertyChanged(ref properties, value); }

        public PropertyInfo Selected { get; set; }

        private string _controlTitle;
        public string ControlTitle
        {
            get
            {
                return _controlTitle;
            }
            set
            {
                SetAndRaisePropertyChanged(ref _controlTitle, value);
            }
        }

        private View _element;
        private ObservableCollection<PropertyInfo> properties;

        public ControlPageViewModel()
        {
            ViewXAMLCommand = new Command(ViewXAML);
            UndoCommand = new Command(Undo);
            RedoCommand = new Command(Redo);
            ResetCommand = new Command(Reset);
            SelectCommand = new Command(OnPropertySelect);
            SwitchedCommand = new Command<PropertyInfo>(OnSwitchToggled);
        }

        private void OnSwitchToggled(PropertyInfo propertyInfo)
        {
            var currentValue = (bool)propertyInfo.GetValue(_element);
            propertyInfo.SetValue(_element, !currentValue);
        }

        private void OnPropertySelect()
        {
            // need to push a new property panel into that view
        }

        private async void ViewXAML()
        {
            var source = XamlUtil.GetXamlForType(typeof(ControlPage));
            await Shell.Current.Navigation.PushAsync(new ViewSourcePage
            {
                Source = source
            });
        }

        private void Undo()
        {

        }

        private void Redo()
        {

        }

        private void Reset()
        {

        }

        public void SetElement(View view, HashSet<string> exceptions)
        {
            _element = view;

            var elementType = _element.GetType();

            var publicProperties = elementType
                .GetProperties(BindingFlags.Public
                    | BindingFlags.Instance
                    | BindingFlags.FlattenHierarchy)
                .Where(p => p.CanRead && p.CanWrite && !exceptions.Contains(p.Name));



            // BindableProperty used to clean property values
            //var bindableProperties = elementType
            //    .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            //    .Where(p => p.FieldType.IsAssignableFrom(typeof(BindableProperty)))
            //    .Select(p => (BindableProperty)p.GetValue(_element));

            var props = new ObservableCollection<PropertyInfo>();
            foreach (var property in publicProperties)
            {
                if (
                    property.PropertyType == typeof(Color)
                    || property.PropertyType == typeof(string)
                    || property.PropertyType == typeof(double)
                    || property.PropertyType == typeof(float)
                    || property.PropertyType == typeof(int)
                    || property.PropertyType == typeof(bool)
                    || property.PropertyType == typeof(Thickness)
                    )
                {
                    props.Add(property);
                }
            }

            Properties = new ObservableCollection<PropertyInfo>(props.OrderBy(x => x.Name));

        }

    }

}
