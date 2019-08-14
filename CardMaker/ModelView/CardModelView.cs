using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace CardMaker
{
    /// <summary>
    /// This stores the individual card's info. This also
    /// has extensions used by the tree-viewer.
    /// </summary>
    [Serializable]
    public class CardModelView : ITreeViewItemViewModel
    {
        #region Private Members
        private string _CardType = "Ally";
        private string _Affiliation = "Neutral";
        private string _Title = "<card title>";
        private int _Attack = 10;
        private int _Defense = 10;
        private int _Level = 0;
        private int _Energy = 0;
        private string _Description = "<Card Description>";
        private bool _Exclusive = false;
        private bool _PrintStatus = false;
        #endregion


        #region Public Properties
        public string CardType
        {
            get { return _CardType; }
            set
            {
                if (!_CardType.Equals(value))
                {
                    _CardType = value;
                    NotifyPropertyChanged("CardType");
                }
            }
        }

        public string Affiliation
        {
            get { return _Affiliation; }
            set
            {
                if (!_Affiliation.Equals(value))
                {
                    _Affiliation = value;
                    NotifyPropertyChanged("Affiliation");
                }
            }
        }

        public string Title
        {
            get { return _Title; }
            set
            {
                if (!_Title.Equals(value))
                {
                    _Title = value;
                    NotifyPropertyChanged("Title");
                    NotifyPropertyChanged("Name");
                }
            }
        }

        public int Attack
        {
            get { return _Attack; }
            set
            {
                if (_Attack != value)
                {
                    _Attack = value;
                    NotifyPropertyChanged("Attack");
                }
            }
        }

        public int Defense
        {
            get { return _Defense; }
            set
            {
                if (_Defense != value)
                {
                    _Defense = value;
                    NotifyPropertyChanged("Defense");
                }
            }
        }

        public int Level
        {
            get { return _Level; }
            set
            {
                if (_Level != value)
                {
                    _Level = value;
                    NotifyPropertyChanged("Level");
                }
            }
        }
        
        public int Energy
        {
            get { return _Energy; }
            set
            {
                if (_Energy != value)
                {
                    _Energy = value;
                    NotifyPropertyChanged("Energy");
                }
            }
        }

        public string Description
        {
            get { return _Description; }
            set
            {
                if (!_Description.Equals(value))
                {
                    _Description = value;
                    NotifyPropertyChanged("Description");
                }
            }
        }

        public bool Exclusive
        {
            get { return _Exclusive; }
            set
            {
                if (_Exclusive != value)
                {
                    _Exclusive = value;
                    NotifyPropertyChanged("Exclusive");
                }
            }
        }

        public bool PrintStatus
        {
            get { return _PrintStatus; }
            set
            {
                if (_PrintStatus != value)
                {
                    _PrintStatus = value;
                    //make card-set reflect this change
                    CardSetModelView set = this.Parent as CardSetModelView;
                    set.UpdatePrintStatus(value);
                    NotifyPropertyChanged("PrintStatus");
                }
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        
        #endregion


        #region Tree View Properties
        public ObservableCollection<ITreeViewItemViewModel> Children { get { return null; } }
        private ITreeViewItemViewModel _OwningSet;
        public ITreeViewItemViewModel Parent { get { return _OwningSet; } }
        public string Name
        {
            get { return Title; }
        }
        public bool HasDummyChild { get { return false; } }
        
        private bool _IsSelected = false;
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

        private bool _IsExpanded;
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

        private bool _IsChecked;
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

        public bool IsLeaf { get { return true; } }
        #endregion


        #region Public Methods
        /// <summary>
        /// Required public default constructor for data binding.
        /// </summary>
        public CardModelView(CardSetModelView owningSet)
        {
            _OwningSet = owningSet;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newSet"></param>
        public void TransferOwnership(CardSetModelView newSet)
        {
            this._OwningSet = newSet;
        }

        public void SetOwner(CardSetModelView set)
        {
            this._OwningSet = set;
        }
        #endregion


        #region Private Methods
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
