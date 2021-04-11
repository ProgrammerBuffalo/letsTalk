using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModels
{
    public class AvailableUsersViewModel : INotifyPropertyChanged
    {
        public string Info { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public AvailableUsersViewModel()
        {
            
        }
    }
}
