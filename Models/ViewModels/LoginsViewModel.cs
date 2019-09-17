using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace codetest.Models.ViewModels
{
    public class LoginsViewModel
    {
        public LoginsViewModel()
        {
            Logins = new List<Login>();
        }
        public List<Login> Logins { get; set; }
    }
}
