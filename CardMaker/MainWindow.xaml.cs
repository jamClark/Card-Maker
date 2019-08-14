using System;
using System.Collections.Generic;
using System.Linq;
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
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace CardMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, INotifyCollectionChanged, ICardSetsCollection
    {
        #region Private Members
        private CardModelView _ActiveCard;
        private IList<CardSetModelView> _CardSets;
        #endregion


        #region Public Properties
        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void NotifyCollectionChanged(NotifyCollectionChangedAction action)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, _CardSets));
            }
        }

        public CardModelView ActiveCard
        { 
            get {return _ActiveCard;} 
            set
            {
                if (_ActiveCard != value)
                {
                    _ActiveCard = value;
                    NotifyPropertyChanged("ActiveCard");
                }
            }
        }

        public IList<CardSetModelView> CardSets
        {
            get
            {
                return _CardSets;
            }
            set
            {
                if (_CardSets != value)
                {
                    _CardSets = value;
                    NotifyPropertyChanged("CardSets");
                    NotifyPropertyChanged("ObservableCardSets");
                }
            }
        }

        public ObservableCollection<ITreeViewItemViewModel> ObservableCardSets
        {
            get { return new ObservableCollection<ITreeViewItemViewModel>(CardSets); }
        }

        #endregion


        #region Methods
        /// <summary>
        /// 
        /// </summary>
        public MainWindow()
        {
            CardSets = new List<CardSetModelView>();
            CardSets.Add(new CardSetModelView());
            ActiveCard = new CardModelView(CardSets[0]);
            CardSets[0].Cards.Add(ActiveCard);
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {

            System.Windows.Data.CollectionViewSource cardDefinitionViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("cardDefinitionViewSource")));
            // Load data by setting the CollectionViewSource.Source property:
            // cardDefinitionViewSource.Source = [generic data source]
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void renamer_LostFocus(object sender, RoutedEventArgs e)
        {
            if(SelectedSet == null) return;
            if(sender != null) SelectedSet.Name = this.textboxRenamer.Text;
            NotifyPropertyChanged("CardSets");
            NotifyPropertyChanged("ObservableCardSets");

            SelectedSet = null;
            this.textboxRenamer.Visibility = System.Windows.Visibility.Hidden;
            this.textboxRenamer.IsEnabled = false;
            Keyboard.ClearFocus();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void renamer_KeyDown(object sender, KeyboardEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.Enter))
            {
                renamer_LostFocus(sender, null); //store new name
            }
            if (e.KeyboardDevice.IsKeyDown(Key.Escape))
            {
                renamer_LostFocus(null, null); //reset the name
            }
        }

        /// <summary>
        /// Handles right-clicking of TreeView so that context menu uses the current selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trv_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                this.ActiveCard = sender as CardModelView;
                item.Focus();
            }
        }

        /// <summary>
        /// Handles both selection of active card and beginning of dragging events.
        /// </summary>
        Point StartDragPos;
        bool Dragging = false;
        private void treeview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                TreeViewItem item = sender as TreeViewItem;
                ITreeViewItemViewModel itemModel = item.DataContext as ITreeViewItemViewModel;
                if (item != null && itemModel != null && itemModel.IsLeaf)
                {
                    StartDragPos = e.GetPosition(this.treeviewCardSets);
                    Dragging = true;
                    ActiveCard = itemModel as CardModelView;
                    item.Focus();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeview_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Dragging = false;
            }
        }

        /// <summary>
        /// Checks for confirmation of dragging events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ITreeViewItemViewModel DropTarget;
        ITreeViewItemViewModel DraggedItem;
        private void treeview_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Dragging) return;
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point currentMousePos = e.GetPosition(this.treeviewCardSets);
                    if (Math.Abs(currentMousePos.X - StartDragPos.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(currentMousePos.Y - StartDragPos.Y) > SystemParameters.MinimumVerticalDragDistance)
                    {
                        DraggedItem = this.treeviewCardSets.SelectedItem as ITreeViewItemViewModel;
                        if (DraggedItem != null)
                        {
                            DragDropEffects finalDropEffect = DragDrop.DoDragDrop(this.treeviewCardSets,
                                                                                  this.treeviewCardSets.SelectedValue,
                                                                                  DragDropEffects.Move);
                                
                            //ensure target is not null and item is dragging
                            if (finalDropEffect == DragDropEffects.Move && DropTarget != null)
                            {
                                if (DropTarget.IsLeaf)
                                {
                                    CardSetModelView set = DropTarget.Parent as CardSetModelView;
                                    set.TransferCard(DraggedItem as CardModelView);
                                    DropTarget = null;
                                    DraggedItem = null;
                                }
                                else
                                {
                                    //copy card to target set
                                    //then remove references
                                    CardSetModelView set = DropTarget as CardSetModelView;
                                    set.TransferCard(DraggedItem as CardModelView);
                                    DropTarget = null;
                                    DraggedItem = null;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception )
            {
                
            }
            return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeview_DragOver(object sender, DragEventArgs e)
        {
            if (!Dragging) return;

            try
            {
               Point currentMousePos = e.GetPosition(this.treeviewCardSets);
               if (Math.Abs(currentMousePos.X - StartDragPos.X) > SystemParameters.MinimumHorizontalDragDistance ||
                   Math.Abs(currentMousePos.Y - StartDragPos.Y) > SystemParameters.MinimumVerticalDragDistance)
               {
                    //verify that this is a valid drop target and then store the drop target
                    TreeViewItem item = GetNearestContainer(e.OriginalSource as UIElement);
                    ITreeViewItemViewModel itemModel = item.DataContext as ITreeViewItemViewModel;
                    if (item != null && itemModel != null && !itemModel.IsLeaf)
                    {
                        e.Effects = DragDropEffects.Move;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                    }
               }

            }
            catch (Exception)
            { }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeview_Drop(object sender, DragEventArgs e)
        {
            if (!Dragging) return;

            try
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;

                //verify this is a valid drop then store the drop target
                TreeViewItem item = GetNearestContainer(e.OriginalSource as UIElement);
                ITreeViewItemViewModel itemModel = item.DataContext as ITreeViewItemViewModel;
                
                if(item != null && itemModel != null)
                {
                    DropTarget = itemModel;
                    e.Effects = DragDropEffects.Move;
                }

            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private TreeViewItem GetNearestContainer(UIElement element)
        {

            // Walk up the element tree to the nearest tree view item.

            TreeViewItem container = element as TreeViewItem;

            while ((container == null) && (element != null))
            {

                element = VisualTreeHelper.GetParent(element) as UIElement;

                container = element as TreeViewItem;

            }

            return container;

        }
        #endregion

        
        #region SetCollection Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IList<ITreeViewItemViewModel> GetPrintList()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectedSet"></param>
        public void AddCard(ITreeViewItemViewModel selectedSetRoot)
        {
            if (selectedSetRoot == null)
            {
                //imply selection
                ITreeViewItemViewModel selectedSet = this.treeviewCardSets.SelectedValue as ITreeViewItemViewModel;
                if (selectedSet != null)
                {
                    CardSetModelView setModel;
                    if (selectedSet.IsLeaf) setModel = selectedSet.Parent as CardSetModelView;
                    else setModel = selectedSet as CardSetModelView;

                    setModel.Cards.Add(new CardModelView(setModel));
                    NotifyPropertyChanged("CardSets");
                    NotifyPropertyChanged("ObservableCardSets");
                    setModel.IsExpanded = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddSet()
        {
            CardSetModelView newSet = new CardSetModelView();
            newSet.ResetToDefaultSet();
            CardSets.Add(newSet);
            NotifyPropertyChanged("CardSets");
            NotifyPropertyChanged("ObservableCardSets");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectedItem"></param>
        public void RemoveCard(ITreeViewItemViewModel selectedItem)
        {
            if (selectedItem == null)
            {
                //imply selection
                selectedItem = this.treeviewCardSets.SelectedValue as ITreeViewItemViewModel;
                if (selectedItem != null)
                {
                    CardSetModelView setModel;
                    if (selectedItem.IsLeaf) setModel = selectedItem.Parent as CardSetModelView;
                    else return;

                    setModel.Cards.Remove(selectedItem);
                    NotifyPropertyChanged("CardSets");
                    NotifyPropertyChanged("ObservableCardSets");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectedItem"></param>
        public void RemoveSet(ITreeViewItemViewModel selectedItem)
        {
            if (selectedItem == null)
            {
                //imply selection
                selectedItem = this.treeviewCardSets.SelectedValue as ITreeViewItemViewModel;
                if (selectedItem != null)
                {
                    CardSetModelView setModel;
                    if (!selectedItem.IsLeaf) setModel = selectedItem as CardSetModelView;
                    else return;

                    CardSets.Remove(setModel);
                    NotifyPropertyChanged("CardSets");
                    NotifyPropertyChanged("ObservableCardSets");
                }
            }
            else
            {
                CardSetModelView set = selectedItem as CardSetModelView;
                if (set != null && CardSets.Contains(set))
                {
                    CardSets.Remove(set);
                    NotifyPropertyChanged("CardSets");
                    NotifyPropertyChanged("ObservableCardSets");
                }

            
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectedSet"></param>
        CardSetModelView SelectedSet = null;
        public void RenameSet()
        {
            //imply selection
            ITreeViewItemViewModel selectedItem = this.treeviewCardSets.SelectedValue as ITreeViewItemViewModel;
            if (selectedItem != null)
            {
                CardSetModelView setModel;
                if (!selectedItem.IsLeaf) setModel = selectedItem as CardSetModelView;
                else return;

                TreeViewItem item = this.treeviewCardSets.ItemContainerGenerator.ContainerFromItem(this.treeviewCardSets.SelectedItem) as TreeViewItem;
                //myTree.ItemContainerGenerator.ContainerFromItem(SelectedItem) as TreeViewItem
                if (item == null)
                {
                    MessageBox.Show("Oh no");
                    return;
                }
                GeneralTransform myTransform = item.TransformToAncestor(treeviewCardSets);
                Point myOffset = myTransform.Transform(new Point(0, 0));
                // myOffset.Y contains your distance from the top of the treeview now
                textboxRenamer.Margin = new Thickness(10.0,myOffset.Y+32.0,0,0);
                SelectedSet = setModel;
                
                this.textboxRenamer.Visibility = System.Windows.Visibility.Visible;
                this.textboxRenamer.IsEnabled = true;
                this.textboxRenamer.Text = setModel.Name;
                this.textboxRenamer.Focus();
            }
        }

       /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveWorkspace(string fileName)
        {
            if (fileName != null && fileName.Length > 0)
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
                formatter.Serialize(stream, _CardSets);
                stream.Close();
            }
        }

        public void OpenWorkspace(string fileName)
        {
            if (fileName != null && fileName.Length > 0)
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                _CardSets = (List<CardSetModelView>)formatter.Deserialize(stream);
                stream.Close();

                NotifyPropertyChanged("CardSets");
                NotifyPropertyChanged("ObservableCardSets");
                if(_CardSets.Count > 0 && _CardSets[0].Cards.Count > 0) ActiveCard = _CardSets[0].Cards[0] as CardModelView;
            }
        }

        public void AppendWorkspace(string fileName)
        {
            if (fileName != null && fileName.Length > 0)
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                IList<CardSetModelView> tempSets = (List<CardSetModelView>)formatter.Deserialize(stream);
                stream.Close();

                List<CardSetModelView> mainSets = _CardSets as List<CardSetModelView>;
                mainSets.AddRange(tempSets);

                NotifyPropertyChanged("CardSets");
                NotifyPropertyChanged("ObservableCardSets");
                if (_CardSets.Count > 0 && _CardSets[0].Cards.Count > 0) ActiveCard = _CardSets[0].Cards[0] as CardModelView;
            }
        }

        public void ExportCard(string fileName)
        {
            if (fileName != null && fileName.Length > 0)
            {
                //imply selection
                ITreeViewItemViewModel selectedItem = this.treeviewCardSets.SelectedValue as ITreeViewItemViewModel;

                if (selectedItem != null)
                {
                    CardModelView card;
                    if (selectedItem.IsLeaf) card = selectedItem as CardModelView;
                    else
                    {
                        MessageBox.Show("There was an error saving the card. " + fileName);
                        return;
                    }

                    //serialize that card
                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
                    formatter.Serialize(stream, card);
                    stream.Close();
                    //stream.Dispose();
                }
                else
                {
                    MessageBox.Show("There was an error saving the card. " + fileName);
                }
                
            }
        }

        public void ImportCard(string fileName)
        {
            if (fileName != null && fileName.Length > 0)
            {
                //imply selection
                ITreeViewItemViewModel selectedItem = this.treeviewCardSets.SelectedValue as ITreeViewItemViewModel;

                if (selectedItem != null)
                {
                    CardSetModelView cardSet;
                    if (selectedItem.IsLeaf) cardSet = selectedItem.Parent as CardSetModelView;
                    else cardSet = selectedItem as CardSetModelView;

                    //serialize that card
                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    CardModelView card = (CardModelView)formatter.Deserialize(stream);
                    stream.Close();

                    if (card == null)
                    {
                        MessageBox.Show("The card did not load properly. " + fileName);
                        return;
                    }
                    cardSet.AddCardToSet(card);
                    NotifyPropertyChanged("CardSets");
                    NotifyPropertyChanged("ObservableCardSets");
                    cardSet.IsExpanded = true;
                    ActiveCard = card;
                }
                else
                {
                    MessageBox.Show("There was an error loading the card. " + fileName);
                }

            }
        }

        public void ExportSet(string fileName)
        {
            if (fileName != null && fileName.Length > 0)
            {
                //imply selection
                ITreeViewItemViewModel selectedItem = this.treeviewCardSets.SelectedValue as ITreeViewItemViewModel;

                if (selectedItem != null)
                {
                    CardSetModelView cardSet;
                    if (!selectedItem.IsLeaf) cardSet = selectedItem as CardSetModelView;
                    else cardSet = selectedItem.Parent as CardSetModelView;

                    //serialize that card
                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
                    formatter.Serialize(stream, cardSet);
                    stream.Close();
                    //XmlSerializer serializer = new XmlSerializer(cardSet.GetType());
                    //TextWriter writer = new StreamWriter(fileName, false, Encoding.UTF8);
                    //serializer.Serialize(writer, cardSet);
                    //writer.Close();
                }
                else
                {
                    MessageBox.Show("There was an error saving the card set. " + fileName);
                }
            }
        }

        public void ImportSet(string fileName)
        {
            if (fileName != null && fileName.Length > 0)
            {
                //imply selection
                ITreeViewItemViewModel selectedItem = this.treeviewCardSets.SelectedValue as ITreeViewItemViewModel;

                if (selectedItem != null)
                {
                    //CardSetModelView cardSet;
                    //if (selectedItem.IsLeaf) cardSet = selectedItem.Parent as CardSetModelView;
                    //else cardSet = selectedItem as CardSetModelView;

                    //serialize that card
                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    CardSetModelView cardSet = (CardSetModelView)formatter.Deserialize(stream);
                    stream.Close();

                    if (cardSet == null)
                    {
                        MessageBox.Show("The card set did not load properly. " + fileName);
                        return;
                    }
                    //cardSet.AddCardToSet(cardSet);
                    this.CardSets.Add(cardSet);
                    NotifyPropertyChanged("CardSets");
                    NotifyPropertyChanged("ObservableCardSets");
                    cardSet.IsExpanded = true;
                    ActiveCard = cardSet.Cards[0] as CardModelView;
                }
                else
                {
                    MessageBox.Show("There was an error loading the card. " + fileName);
                }
            }
        }

        public void PrintCards()
        {
            
            //first, let's figure out how many cards we will have total
            List<CardModelView> cards = new List<CardModelView>();
            foreach (CardSetModelView set in _CardSets)
            {
                foreach (CardModelView card in set.Cards)
                {
                    if(card.PrintStatus) cards.Add(card);
                }
            }

            if (cards.Count < 1)
            {
                MessageBox.Show("You must selected at least one card to print.");
                return;
            }

            PrintDialog dialog = new PrintDialog();
            if (dialog.ShowDialog() != true) return;

            
            double DPI = 96.0;
            CardPaginator paginator = new CardPaginator(
                cards,
                new Size(2.5 * DPI, 3.5 * DPI),
                new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight)
                );
            dialog.PrintDocument(paginator, "Wartorn Test Cards");
        }
        #endregion
    }
}
