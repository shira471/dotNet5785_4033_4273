using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlApi;

namespace PL.viewModel
{
    public class ActionManagerVM: ViewModelBase
    {
        private readonly IBl s_bl = Factory.Get();
        private BO.CallInList? _selectedCall;
        public BO.CallInList? SelectedCall
        {
            get => _selectedCall;
            set
            {
                if (_selectedCall != value)
                {
                    _selectedCall = value;
                    OnPropertyChanged(nameof(SelectedCall));
                }
            }
        }
    }
}
