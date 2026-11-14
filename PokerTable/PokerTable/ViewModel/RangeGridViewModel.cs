using PokerTable.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PokerTable.ViewModel
{
    public class RangeGridViewModel : ViewModelBase
    {
        #region Members
        private List<string> range;
        #endregion
        #region Constructor
        public RangeGridViewModel()
        {
        }
        #endregion
        #region Properties
        public List<string> Range
        {
            get
            {
                return range;
            }
            set
            {
                range = value;
                OnPropertyChanged(nameof(Range));
            }
        }
        #endregion
    }
}
