using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleBase;

namespace SharedLib
{
    public class Address
    {
        public const int Iota_Address_Length = 33;

        public string OwnerName { get; set; }
        public string AddressValue { get; set; }
        public bool IsReceive { get; set; }
        public bool IsVerified { get; set; }

        public Address(string addressValue, bool isReceive)
        {
            addressValue = addressValue.Trim();
            if (!IsIotaAddress(addressValue))
                throw new ArgumentException($"Address addressValue {addressValue} is not in correct format");

            OwnerName = "Anonymous";
            AddressValue = addressValue;
            IsReceive = isReceive;
        }

        [JsonConstructor]
        public Address(string ownerName, string addressValue, bool isReceive) : this(addressValue, isReceive)
        {
            OwnerName = ownerName;
        }

        public override string ToString()
        {
            return $"{OwnerName} {AddressValue}";
        }

        public static bool IsIotaAddress(string addressValue)
        {
            String decodedAddress;

            addressValue = addressValue.Trim();
            return TryDecode(addressValue, out decodedAddress) &&
                   (decodedAddress.Length == Iota_Address_Length * 2); //addressValue must be 33 bytes (base58 encoded)
        }

        public static bool TryDecode(string addressValue, out string decodedAddressValue)
        {
            decodedAddressValue = "";
            try
            {
                decodedAddressValue = Decode(addressValue);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        static string Decode(string addressValue)
        {
            Span<byte> result = Base58.Bitcoin.Decode(addressValue);
            string s = BitConverter.ToString(result.ToArray());
            return s.Replace("-", "");
        }

        public override bool Equals(object? obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Address otherAddress = (Address)obj;
                return (AddressValue == otherAddress.AddressValue); //addresses are equal when their addressValue is the same
            }
        }

        public override int GetHashCode()
        {
            return AddressValue.GetHashCode();
        }
    }
}
