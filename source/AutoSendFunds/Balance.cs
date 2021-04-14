using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTA_Pollen_AutoSendFunds
{
    public enum BalanceStatus
    {
        Ok,
        Pending
    }

    class Balance
    {
        public BalanceStatus BalanceStatus { get; set; }
        public int BalanceValue { get; set; }
        public string Color { get; set; }
        public string TokenName { get; set; }

        public Balance(BalanceStatus balanceStatus, int balanceValue, string color, string tokenName)
        {
            BalanceStatus = balanceStatus;
            BalanceValue = balanceValue;
            Color = color;
            TokenName = tokenName;
        }

        public override string ToString()
        {
            return $"Balance: status {BalanceStatus}, value {BalanceValue}, color {Color}, tokenName {TokenName}";
        }
    }
}
