﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RecipeSelectHelper.Resources;

namespace RecipeSelectHelper.View.Miscellaneous
{
    /// <summary>
    /// Interaction logic for MassAddGroupedCategoriesPage.xaml
    /// This page is used for modifying items where yes-no questions are not enough.
    /// For instance, adding a new GroupedCategory to every recipe.
    /// It works by turning every item into a UiElement (using Func), displaying this, and then turning the UiElement back into a modified item.
    /// </summary>
    public partial class MassAddGroupedCategoriesPage : Page, INotifyPropertyChanged
    {
        private MainWindow _parent;
        private Func<object, string> _createItemDescription;
        private Func<object, UIElement> _convertItemToUiElement;
        private Action<UIElement, object> _alterOriginalWithResultAndItem;
        private List<UIElement> _modifiedItems;

        public MassAddGroupedCategoriesPage(MainWindow parent, string title, string pageDescription, 
            List<object> itemsToEdit, Func<object, string> createItemDescription,
            Func<object, UIElement> convertItemToUiElement, Action<UIElement, object> alterOriginalWithResultAndItem)
        {
            _parent = parent;

            if (!itemsToEdit.IsNullOrEmpty()) // If empty, it'll initialize the page and then navigate back
            {
                ItemsToEdit = itemsToEdit ?? new List<object>();
                _modifiedItems = new List<UIElement>(ItemsToEdit.Count);
                _createItemDescription = createItemDescription;
                _convertItemToUiElement = convertItemToUiElement;
                _alterOriginalWithResultAndItem = alterOriginalWithResultAndItem;
                PageTitle = title;
                PageDescription = pageDescription;
            }

            Loaded += MassAddGroupedCategoriesPage_Loaded; ;
            InitializeComponent();
        }

        private void MassAddGroupedCategoriesPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (ItemsToEdit.IsNullOrEmpty()) ClosePage();
            else GetNextObject();
        }

        #region ObservableObjects

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string PageTitle { get; }

        public string PageDescription { get; }

        public IList<object> ItemsToEdit { get; }

        private int _currentIndex = 0;
        public int CurrentIndex
        {
            get { return _currentIndex; }
            private set { _currentIndex = value; OnPropertyChanged(nameof(CurrentIndex)); }
        }

        private string _itemDescription;
        public string ItemDescription
        {
            get { return _itemDescription; }
            set { _itemDescription = value; OnPropertyChanged(nameof(ItemDescription)); }
        }

        private UIElement _currentUiElement;
        public UIElement CurrentUiElement
        {
            get { return _currentUiElement; }
            set { _currentUiElement = value; OnPropertyChanged(nameof(CurrentUiElement)); }
        }

        #endregion

        private void GetNextObject()
        {
            if (ItemsToEdit.Count <= CurrentIndex)
            {
                ApplyAndClosePage();
                return;
            }
            object newItem = ItemsToEdit[CurrentIndex++];
            CurrentUiElement = _convertItemToUiElement(newItem);
            ItemDescription = _createItemDescription(newItem);
        }

        private void ApplyAndClosePage()
        {
            ApplyChanges();
            ClosePage();
        }

        private void ClosePage()
        {
            _parent.ContentControl.NavigationService.GoBack();
        }

        private void ApplyChanges()
        {
            if (_modifiedItems.Count != ItemsToEdit.Count) throw new ArgumentException();
            for (var i = 0; i < _modifiedItems.Count; i++)
            {
                _alterOriginalWithResultAndItem(_modifiedItems[i], ItemsToEdit[i]);
            }
        }

        private void ButtonNext_OnClick(object sender, RoutedEventArgs e)
        {
            _modifiedItems.Add(CurrentUiElement);
            GetNextObject();
        }

        private void ButtonAbort_OnClick(object sender, RoutedEventArgs e)
        {
            ClosePage();
        }
    }
}
