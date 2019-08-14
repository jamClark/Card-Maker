using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace CardMaker
{
    /// <summary>
    /// This class is used to group cards into sets. This can be a container
    /// for cards, or sub-sets, but not both.
    /// </summary>
    [Serializable]
    public class CardSetModelView : ITreeViewItemViewModel
    {
        #region Private Members
        private IList<ITreeViewItemViewModel> _Cards;
        private string _SetName = "Card Set";
        private bool _IsSelected;
        private bool _IsExpanded;
        private bool _IsChecked;
        private bool? _PrintStatus = false;
        private int Version;
        #endregion


        #region Public Properties
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        
        public bool? PrintStatus
        {
            get
            {
                return _PrintStatus;
            }

            set
            {
                //If this is partly checked, make it full-on. Then traverse cards
                //and check all of them.
                //If this is off, make it full on and check all cards as well.
                //If this is full-on, turn it off and make all cards off as well.
                if (_PrintStatus != value)
                {
                    _PrintStatus = value;
                    if (_PrintStatus == false)
                    {
                        foreach (CardModelView card in _Cards) card.PrintStatus = false;
                        
                    }
                    else if(_PrintStatus == true)
                    {
                        foreach (CardModelView card in _Cards) card.PrintStatus = true;
                    }
                    NotifyPropertyChanged("PrintStatus");
                }
            }
        }
        
        public IList<ITreeViewItemViewModel> Cards
        {
            get 
            {
                return _Cards;
            }
            set
            {
                if (_Cards != value)
                {
                    _Cards = value;
                    NotifyPropertyChanged("Cards");
                    NotifyPropertyChanged("Children");
                    NotifyPropertyChanged("ObservableCards");
                }
            }
        }

        public ObservableCollection<ITreeViewItemViewModel> ObservableCards
        {
            get { return new ObservableCollection<ITreeViewItemViewModel>(Cards); }
        }
        #endregion


        #region Tree View Properties
        public ObservableCollection<ITreeViewItemViewModel> Children
        {
            get { return new ObservableCollection<ITreeViewItemViewModel>(_Cards); }
        }

        public ITreeViewItemViewModel Parent
        {
            get { return null; }
        }

        public string Name
        {
            get { return _SetName; }
            set
            {
                if (!_SetName.Equals(value))
                {
                    _SetName = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }
        
        public bool HasDummyChild {get { return true; }}
        
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }

        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set
            {
                if (value != _IsExpanded)
                {
                    _IsExpanded = value;
                    NotifyPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (_IsExpanded && Parent != null) Parent.IsExpanded = true;
            }
        }

        public bool IsChecked
        {
            get { return _IsChecked; }
            set
            {
                if (value != _IsChecked)
                {
                    _IsChecked = value;
                    NotifyPropertyChanged("IsChecked");
                }

                // Expand all the way up to the root.
                //if (_IsChecked && _parent != null)
                //    _parent.IsChecked = true;
            }
        }

        public bool IsLeaf { get { return false; } }
        #endregion


        #region Public Methods
        /// <summary>
        /// Default constructor that is required for binding to work.
        /// </summary>
        public CardSetModelView()
        {
            _Cards = new List<ITreeViewItemViewModel>();
        }

        /// <summary>
        /// Moves a card from the old set to this one
        /// and notifies of property changes.
        /// </summary>
        /// <param name="old"></param>
        public void TransferCard(CardModelView card)
        {
            if (card == null) return;
            CardSetModelView old = card.Parent as CardSetModelView;
            if (old == null) return;
            if (old == this) 
            {
                //MessageBox.Show("Whoa!");
                return;
            }

            _Cards.Add(card);
            old._Cards.Remove(card);
            card.TransferOwnership(this);

            NotifyPropertyChanged("Cards");
            NotifyPropertyChanged("Children");
            old.NotifyPropertyChanged("Cards");
            old.NotifyPropertyChanged("Children");
        }

        /// <summary>
        /// Returns the model to default state of a single card set with a single card within it.
        /// </summary>
        public void ClearSet()
        {
            _Cards.Clear();
            PrintStatus = false;
        }

        /// <summary>
        /// Returns the model to default state of a single card set with a single card within it.
        /// </summary>
        public void ResetToDefaultSet()
        {
            _Cards.Clear();

            PrintStatus = false;
            _Cards = new List<ITreeViewItemViewModel>();
            CardModelView defaultCard = new CardModelView(this);
            _Cards.Add(defaultCard);
        }

        /// <summary>
        /// Called by card model when they change their checked status.
        /// This ensures that the parent set has the correct state based on the cards within.
        /// </summary>
        /// <param name="x"></param>
        public void UpdatePrintStatus(bool x)
        {
            bool foundUnchecked = false;
            bool foundChecked = false;
            //look through all cards contained, if any are checked, this must
            //be checked at least partly. If *all* are checked, this is full-on
            foreach (CardModelView card in _Cards)
            {
                if (card.PrintStatus == false) foundUnchecked = true;
                else foundChecked = true;
            }

            if (foundUnchecked && foundChecked) PrintStatus = null;//mixed
            else if (foundChecked) PrintStatus = true; //all true
            else PrintStatus = false;
        }

        public void AddCardToSet(CardModelView card)
        {
            card.SetOwner(this);
            Cards.Add(card);
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
