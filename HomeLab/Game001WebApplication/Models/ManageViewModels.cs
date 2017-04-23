using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace Game001WebApplication.Models
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }

        [Display(Name = "Псевдоним")]
        public string Login { get; set; }
        [Display(Name = "Пин код")]
        public string PinCode { get; set; }
        [Display(Name = "Раунд")]
        public string RoundID { get; set; }
        //public IList<RoundResult> RoundResults { get; set; }
        public IList<Bet> Bets { get; set; }

        public IndexViewModel()
        { }

        public class Bet
        {
            public string AssetName { get; set; }
            [Display(Name = "Объём покупки")]
            public double Volume { get; set; }
            [Display(Name = "Цена покупки")]
            public double Price { get; set; }
            [Display(Name = "Особые условия")]
            public string Addition { get; set; }

        }

    }

}