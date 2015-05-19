using System;

namespace JetBlack.Examples.RxProperties.Test
{
    public class Person : NotificationObject
    {
        private string _name;
        private DateTime _dateOfBirth;

        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(() => Name);}
        }

        public DateTime DateOfBirth
        {
            get { return _dateOfBirth; }
            set { _dateOfBirth = value; RaisePropertyChanged(() => DateOfBirth);}
        }
    }
}
