﻿using MCDSaveEdit.Save.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
#nullable enable

namespace MCDSaveEdit
{
    /// <summary>
    /// Interaction logic for SelectionWindow.xaml
    /// </summary>
    public class SelectionWindow : Window
    {
        private ListBox _listBox = new ListBox();
        private bool _isProcessing = true;

        public Action<string?>? onSelection;

        public string? selectedItem {
            get {
                if(_listBox.SelectedItem is ListBoxItem listItem)
                {
                    return listItem.Tag as string;
                }
                return _listBox.SelectedItem as string;
            }
        }

        public SelectionWindow()
        {
            WindowStyle = WindowStyle.ToolWindow;
            ResizeMode = ResizeMode.NoResize;
            Height = 600;
            Width = 300;
            _listBox.SelectionChanged += ListBox_SelectionChanged;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isProcessing) return;
            _listBox.SelectionChanged -= ListBox_SelectionChanged;
            onSelection?.Invoke(selectedItem);
            this.Close();
        }

        public void loadEnchantments(string? selectedEnchantment = null)
        {
            Title = "Select Enchantment";
            Content = _listBox;
            _isProcessing = true;
            _listBox.Items.Clear();

            var powerfulImageSource = ImageUriHelper.instance.imageSource("/Dungeons/Content/UI/Materials/Inventory2/Enchantment/Inspector/element_powerful");

            foreach (var enchantment in EnchantmentExtensions.allEnchantments.OrderBy(str => str).Concat(new[] { "unset" }))
            {
                var imageSource = ImageUriHelper.instance.imageSourceForEnchantment(enchantment);
                if (imageSource == null)
                {
                    continue;
                }

                var image = new Image {
                    Height = 50,
                    Width = 50,
                    VerticalAlignment = VerticalAlignment.Center,
                    Source = imageSource,
                };
                var label = new Label {
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14,
                    Content = enchantment,
                };

                var stackPanel = new StackPanel {
                    Height = 45,
                    Orientation = Orientation.Horizontal,
                };

                stackPanel.Children.Add(image);
                stackPanel.Children.Add(label);
                if (EnchantmentExtensions.powerful.Any(str => { return enchantment == str.ToLowerInvariant(); }))
                {
                    var powerfulImage = new Image {
                        Height = 25,
                        Width = 25,
                        Source = powerfulImageSource,
                    };
                    stackPanel.Children.Add(powerfulImage);
                }

                var listItem = new ListBoxItem { Content = stackPanel, Tag = enchantment };
                _listBox.Items.Add(listItem);

                if (selectedEnchantment == enchantment)
                {
                    _listBox.SelectedItem = listItem;
                }
            }

            _isProcessing = false;
        }

        public void loadItems(string? selectedItem = null)
        {
            Title = "Select Item";

            var anyButton = new Button { Margin = new Thickness(5), };
            anyButton.Content = new Image { Source = ImageUriHelper.instance.imageSourceForItem("mysteryboxany") };
            anyButton.Command = new RelayCommand<object>(filterItems);
            anyButton.CommandParameter = ItemFilterEnum.All;

            var meleeButton = new Button { Margin = new Thickness(5), };
            meleeButton.Content = new Image { Source = ImageUriHelper.instance.imageSourceForItem("mysteryboxmelee") };
            meleeButton.Command = new RelayCommand<object>(filterItems);
            meleeButton.CommandParameter = ItemFilterEnum.MeleeWeapons;

            var rangedButton = new Button { Margin = new Thickness(5), };
            rangedButton.Content = new Image { Source = ImageUriHelper.instance.imageSourceForItem("mysteryboxranged") };
            rangedButton.Command = new RelayCommand<object>(filterItems);
            rangedButton.CommandParameter = ItemFilterEnum.RangedWeapons;

            var armorButton = new Button { Margin = new Thickness(5), };
            armorButton.Content = new Image { Source = ImageUriHelper.instance.imageSourceForItem("mysteryboxarmor") };
            armorButton.Command = new RelayCommand<object>(filterItems);
            armorButton.CommandParameter = ItemFilterEnum.Armor;

            var artifactButton = new Button { Margin = new Thickness(5), };
            artifactButton.Content = new Image { Source = ImageUriHelper.instance.imageSourceForItem("mysteryboxartifact") };
            artifactButton.Command = new RelayCommand<object>(filterItems);
            artifactButton.CommandParameter = ItemFilterEnum.Artifacts;

            var toolStack = new StackPanel {
                Height = 50,
                HorizontalAlignment = HorizontalAlignment.Center,
                Orientation = Orientation.Horizontal,
            };
            toolStack.Children.Add(anyButton);
            toolStack.Children.Add(meleeButton);
            toolStack.Children.Add(rangedButton);
            toolStack.Children.Add(armorButton);
            toolStack.Children.Add(artifactButton);

            var mainStack = new DockPanel();
            DockPanel.SetDock(toolStack, Dock.Top);
            mainStack.Children.Add(toolStack);
            DockPanel.SetDock(_listBox, Dock.Bottom);
            mainStack.Children.Add(_listBox);

            Content = mainStack;

            buildItemList(selectedItem: selectedItem);
        }

        private void filterItems(object filter)
        {
            if(filter is ItemFilterEnum filterEnum)
            {
                buildItemList(filterEnum);
            }
        }

        private void buildItemList(ItemFilterEnum filter = ItemFilterEnum.All, string? selectedItem = null)
        {
            _isProcessing = true;
            _listBox.Items.Clear();

            foreach (var item in itemsForFilter(filter).OrderBy(str => str))
            {
                var imageSource = ImageUriHelper.instance.imageSourceForItem(item);
                if (imageSource == null)
                {
                    continue;
                }

                var stackPanel = createStackPanel(imageSource, item);
                var listItem = new ListBoxItem { Content = stackPanel, Tag = item };
                _listBox.Items.Add(listItem);

                if (selectedItem == item)
                {
                    _listBox.SelectedItem = listItem;
                }
            }
            _isProcessing = false;
        }

        private IEnumerable<string> itemsForFilter(ItemFilterEnum filter)
        {
            switch(filter)
            {
                case ItemFilterEnum.Artifacts: return ItemExtensions.artifacts;
                case ItemFilterEnum.Armor: return ItemExtensions.armor;
                case ItemFilterEnum.MeleeWeapons: return ItemExtensions.meleeWeapons;
                case ItemFilterEnum.RangedWeapons: return ItemExtensions.rangedWeapons;
                case ItemFilterEnum.All: return ItemExtensions.all;
            }
            return new string[0];
        }

        private StackPanel createStackPanel(BitmapImage? imageSource, string labelText)
        {
            var image = new Image {
                Height = 50,
                Width = 50,
                VerticalAlignment = VerticalAlignment.Center,
                Source = imageSource,
            };
            var label = new Label {
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14,
                Content = labelText,
            };

            var stackPanel = new StackPanel {
                Height = 50,
                Orientation = Orientation.Horizontal,
            };

            stackPanel.Children.Add(image);
            stackPanel.Children.Add(label);

            return stackPanel;
        }
    }
}
