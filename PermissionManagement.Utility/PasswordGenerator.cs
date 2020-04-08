using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace PermissionManagement.Utility
{
  public sealed class PasswordGenerator
    {
        private RNGCryptoServiceProvider rng = null;
        private int _minNumbersCount = 1;
        private int _minUpperCount = 1;
        private int _minLowerCount = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        public PasswordGenerator()
        {
            rng = new RNGCryptoServiceProvider();
        }

        #region Properties

        public int MinimumNumbersCount
        {
            get { return _minNumbersCount; }
            set
            {
                if (value < 0) throw new ArgumentException("value cannot be less than 0.");
                _minNumbersCount = value;
            }
        }
        public int MinimumUpperCaseLettersCount
        {
            get { return _minUpperCount; }
            set
            {
                if (value < 0) throw new ArgumentException("value cannot be less than 0.");
                _minUpperCount = value;
            }
        }
        public int MinimumLowerCaseLettersCount
        {
            get { return _minLowerCount; }
            set
            {
                if (value < 0) throw new ArgumentException("value cannot be less than 0.");
                _minLowerCount = value;
            }
        }

        #endregion

        public string GeneratePassword(int length)
        {
            return GeneratePassword(length, true, true, true, true);
        }

        public string GeneratePassword(int length, bool allowSymbols, bool forceNumbers, bool forceUpper, bool forceLower)
        {
            CheckArguments(length, forceNumbers, forceUpper, forceLower);

            char[] password = GetPassword(length, allowSymbols, forceNumbers, forceUpper, forceLower);

            return new String(password);
        }

        public string GeneratePassword(char[] dictionary, int length, bool forceNumbers, bool forceUpper, bool forceLower)
        {
            CheckArguments(length, forceNumbers, forceUpper, forceLower);

            char[] password = GetPassword(dictionary, length, forceNumbers, forceUpper, forceLower);

            return new String(password);
        }

        private char[] GetPassword(int length, bool allowSymbols, bool forceNumbers, bool forceUpper, bool forceLower)
        {
            char[] simpleDic = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            char[] complexDic = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_-@=+*/$%&!:?.*^[](){}\\|`#~".ToCharArray();

            if (!allowSymbols)
                return GetPassword(simpleDic, length, forceNumbers, forceUpper, forceLower);
            else
                return GetPassword(complexDic, length, forceNumbers, forceUpper, forceLower);
        }

        private void CheckArguments(int length, bool forceNumbers, bool forceUpper, bool forceLower)
        {
            if (length < 0)
                throw new ArgumentException("length cannot be zero.");

            if (forceNumbers || forceUpper || forceLower)
            {
                int minPassLenght = 0;
                if (forceNumbers)
                    minPassLenght += _minNumbersCount;
                if (forceUpper)
                    minPassLenght += _minUpperCount;
                if (forceLower)
                    minPassLenght += _minLowerCount;

                if (length < minPassLenght)
                    throw new ArgumentException("Specified lenght is less than acceptable minimum of = " + minPassLenght);
            }
        }

        private char[] GetPassword(char[] dictionary, int length, bool forceNumbers, bool forceUpper, bool forceLower)
        {
            char[] pass = new char[length];
            for (int i = 0; i < pass.Length; i++)
            {
                pass[i] = dictionary[GetCryptographicRandomNumber(0, dictionary.Length)];

                if (i == (length - 1) && (forceNumbers || forceUpper || forceLower))
                {
                    int numCount = 0;
                    int upperCount = 0;
                    int lowerCount = 0;

                    for (int n = 0; n < pass.Length; n++)
                    {
                        numCount += char.IsNumber(pass[n]) ? 1 : 0;
                        upperCount += char.IsUpper(pass[n]) ? 1 : 0;
                        lowerCount += char.IsLower(pass[n]) ? 1 : 0;
                    }

                    if ((forceNumbers && numCount < _minNumbersCount) ||
                        (forceUpper && upperCount < _minUpperCount) ||
                        (forceLower && lowerCount < _minLowerCount))
                    {
                        i = -1;
                    }
                }
            }
            return pass;
        }

        private int GetCryptographicRandomNumber(int lBound, int uBound)
        {
            if (!(lBound >= 0 && lBound < uBound))
                throw new ArgumentException("Lower bound must be less than upper bound");

            uint urndnum;
            byte[] rndnum = new Byte[4];
            if (lBound == uBound - 1)
            {
                return lBound;
            }

            uint xcludeRndBase = (uint.MaxValue -
                (uint.MaxValue % (uint)(uBound - lBound)));

            do
            {
                rng.GetBytes(rndnum);
                urndnum = System.BitConverter.ToUInt32(rndnum, 0);
            } while (urndnum >= xcludeRndBase);

            return (int)(urndnum % (uBound - lBound)) + lBound;
        }
    }

  public enum PasswordScore
  {
    Blank = 0,
    VeryWeak = 1,
    Weak = 2,
    Medium = 3,
    Strong = 4,
    VeryStrong = 5
  }

  public class PasswordAdvisor
  {
      public static PasswordScore CheckStrength(string password)
      {
          int score = 0;
          if (string.IsNullOrEmpty(password))
          {
              return (PasswordScore)score;
          }
          if (password.Length < 1)
              return PasswordScore.Blank;
          if (password.Length < 4)
              return PasswordScore.VeryWeak;

          if (password.Length >= 8)
              score++;

          Regex expression = new Regex(".*[0-9]+.*", RegexOptions.Compiled | RegexOptions.Multiline);

          if (expression.IsMatch(password))
              score++;

          Regex expression1 = new Regex(".*[a-z]+.*", RegexOptions.Compiled | RegexOptions.Multiline);
          expression = new Regex(".*[A-Z]+.*", RegexOptions.Compiled | RegexOptions.Multiline);

          if (expression.IsMatch(password) && expression1.IsMatch(password))
              score++;

          expression = new Regex(".*[!,@,#,$,%,^,&,*,?,_,~,-,£,(,)]+.*", RegexOptions.Compiled | RegexOptions.Multiline);
          if (expression.IsMatch(password))
              score++;

          if (password.Length >= 12 && score == 4)
              score++;

          return (PasswordScore)score;
      }
  }

}
