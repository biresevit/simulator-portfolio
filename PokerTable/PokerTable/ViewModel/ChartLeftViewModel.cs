using PokerTable.Model;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerTable.ViewModel
{
    public class ChartLeftViewModel : BindableBase
    {
        private TestModel testModel = new();
        public ChartLeftViewModel()
        {
            TestModel = new();
            TestModel.DataList = new List<QueryDBLib.StatResult>();
        }
        public TestModel TestModel
        {
            get { return testModel; }
            set { SetProperty(ref testModel, value); }
        }
    }
}
