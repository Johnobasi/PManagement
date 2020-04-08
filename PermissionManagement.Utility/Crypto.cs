using System;
using System.Security.Cryptography;
using System.Text;


namespace PermissionManagement.Utility
{
       
	#region "vb.net version"

	/// <copyright>Max Sense Limited 2009-2010</copyright>
	/// <summary>
	/// A helper class used to deal with the encryption and decryption of data
	/// in a manner that is consistant and compatible with Java.
	/// </summary>
	/// <remarks>
	/// This code was derived from code found on the web in a SUN forum 
	/// here: http://forums.sun.com/thread.jspa?threadID=603209. The following
	/// comments are taken from Wikipedia.
	/// <para/>
	/// In cryptography, the Advanced Encryption Standard (AES) is an encryption 
	/// standard adopted by the U.S. government. The standard comprises three block 
	/// ciphers, AES-128, AES-192 and AES-256, adopted from a larger collection 
	/// originally published as Rijndael. Each AES cipher has a 128-bit block size, 
	/// with key sizes of 128, 192 and 256 bits, respectively. The AES ciphers have 
	/// been analyzed extensively and are now used worldwide, as was the case with 
	/// its predecessor, the Data Encryption Standard (DES).
	/// <para/>
	/// AES was announced by National Institute of Standards and Technology (NIST) 
	/// as U.S. FIPS PUB 197 (FIPS 197) on November 26, 2001 after a 5-year 
	/// standardization process in which fifteen competing designs were presented and 
	/// evaluated before Rijndael was selected as the most suitable (see Advanced 
	/// Encryption Standard process for more details). It became effective as a standard 
	/// May 26, 2002. As of 2009, AES is one of the most popular algorithms 
	/// used in symmetric key cryptography.  It is available in many different 
	/// encryption packages. AES is the first publicly accessible and open  cipher 
	/// approved by the NSA for top secret information (see Security of AES, below).
	/// <para/>
	/// The Rijndael cipher was developed by two Belgian cryptographers, Joan Daemen 
	/// and Vincent Rijmen, and submitted by them to the AES selection process. 
	/// Rijndael is a portmanteau of the names of the two inventors.
	/// </remarks>
	/// <history>
	///		<change author="Gboyega Suleman" date="18/12/2009">Original Version</change>
	/// </history>
	public sealed class Crypto
	{
		private Crypto()
		{
		}
		/// <summary>
		/// The key for the encyption and decryption
		/// </summary>

		private static byte[] CryptoKey = System.Text.Encoding.UTF8.GetBytes("C@n@ryWharf$900");
		public static string Encrypt(string text)
		{
			return Encrypt(text, CryptoKey);
		}

		public static string Decrypt(string text)
		{
			return Decrypt(text, CryptoKey);
		}

		/// <summary>
		/// Encrypt a text string using an AES-128 cipher.
		/// </summary>
		/// <remarks>
		/// A corresponding Java version of this code is included in this class as a comment and 
		/// is shown below.
		/// </remarks>
		/// <param name="text">The text string to be encrypted.</param>
		/// <param name="password">The password used in the encrypted process.</param>
		/// <returns>The encrypted string.</returns>
		public static string Encrypt(string text, byte[] password)
		{

			if (string.IsNullOrEmpty(text)) {
				return null;
			}

			byte[] keyBytes = new byte[16];
			int len = password.Length;
			if (len > keyBytes.Length) {
				len = keyBytes.Length;
			}
			System.Array.Copy(password, keyBytes, len);

			using (var cipher = new RijndaelManaged()) {
				cipher.Mode = CipherMode.CBC;
				cipher.Padding = PaddingMode.PKCS7;
				cipher.KeySize = 128;
				cipher.BlockSize = 128;
				cipher.Key = keyBytes;
				cipher.IV = keyBytes;

				ICryptoTransform transform = cipher.CreateEncryptor();

				byte[] plainText = Encoding.UTF8.GetBytes(text);
				byte[] cipherBytes = transform.TransformFinalBlock(plainText, 0, plainText.Length);

				return Convert.ToBase64String(cipherBytes);
			}
		}

		/// <summary>
		/// Decrypt a text string using the AES standard.
		/// </summary>
		/// <remarks>
		/// A corresponding Java version of this code is included in this class as a comment 
		/// and is shown below.
		/// </remarks>
		/// <param name="text">The encrypted text string to be decrypted</param>
		/// <param name="password">The password used in the decyption process.</param>
		/// <returns>The decrypted string.</returns>
		public static string Decrypt(string text, byte[] password)
		{

			if (string.IsNullOrEmpty(text)) {
				return null;
			}

			byte[] encryptedData = Convert.FromBase64String(text);

			byte[] keyBytes = new byte[16];
			int len = password.Length;
			if (len > keyBytes.Length) {
				len = keyBytes.Length;
			}
			System.Array.Copy(password, keyBytes, len);

			using (var cipher = new RijndaelManaged()) {
				cipher.Mode = CipherMode.CBC;
				cipher.Padding = PaddingMode.PKCS7;
				cipher.KeySize = 128;
				cipher.BlockSize = 128;
				cipher.Key = keyBytes;
				cipher.IV = keyBytes;

				ICryptoTransform transform = cipher.CreateDecryptor();

				byte[] plainText = transform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

				return Encoding.UTF8.GetString(plainText);
			}
		}
	}
}

#endregion

#region "Java version"

//
//    package crypttest;
// 
//    import java.io.IOException;
//    import java.io.UnsupportedEncodingException;
//    import java.security.InvalidAlgorithmParameterException;
//    import java.security.InvalidKeyException;
//    import java.security.NoSuchAlgorithmException;
//     
//    import javax.crypto.BadPaddingException;
//    import javax.crypto.Cipher;
//    import javax.crypto.IllegalBlockSizeException;
//    import javax.crypto.NoSuchPaddingException;
//    import javax.crypto.spec.IvParameterSpec;
//    import javax.crypto.spec.SecretKeySpec;
//     
//    import sun.misc.BASE64Decoder;
//    import sun.misc.BASE64Encoder;
//
//    public class IDMCrypt 
//    {
//	    private static final byte[] initVectorData ={(byte)50,(byte)51,(byte)52,(byte)53,(byte)54,(byte)55,(byte)56,(byte)57};
//     
//	    public static void main(String[] args) 
//        {
//		    try
//            {
//		        String text = "password";
//		        String password = "test";
//		        String encrypted = encrypt(text,password);
//		        System.out.println(text + " encrypted is " + encrypted );
//		        String decrypted = decrypt(encrypted,password);
//		        System.out.println(encrypted + " decrypted is " + decrypted );
//		    }
//            catch (Exception e)
//            {
//			    e.printStackTrace();
//		    }
//	    }
//     
//	    public static String encrypt(String text, String password) throws Exception
//        {
//		    Cipher cipher = Cipher.Resolve("AES/CBC/PKCS5Padding");
//    		
//		    //setup key
//		    byte[] keyBytes= new byte[16];
//	  	    byte[] b= password.getBytes("UTF-8");
//	  	    int len= b.length; 
//	  	    if (len > keyBytes.length) len = keyBytes.length;
//	  	    System.arraycopy(b, 0, keyBytes, 0, len);
//    		
//		    SecretKeySpec keySpec = new SecretKeySpec(keyBytes, "AES");
//    		
//		    //the below may make this less secure, hard code byte array the IV in both java and .net clients
//		    IvParameterSpec ivSpec = new IvParameterSpec(keyBytes);
//    		 
//		    cipher.init(Cipher.ENCRYPT_MODE,keySpec,ivSpec);
//		    byte [] results = cipher.doFinal(text.getBytes("UTF-8"));
//		    BASE64Encoder encoder = new BASE64Encoder();
//		    return encoder.encode(results);
//	    }
//    	
//	    public static String decrypt(String text, String password) throws Exception
//        {
//		    Cipher cipher = Cipher.Resolve("AES/CBC/PKCS5Padding");
//			
//		    //setup key
//		    byte[] keyBytes= new byte[16];
//	  	    byte[] b= password.getBytes("UTF-8");
//	  	    int len= b.length; 
//	  	    if (len > keyBytes.length) len = keyBytes.length;
//	  	    System.arraycopy(b, 0, keyBytes, 0, len);
//		    SecretKeySpec keySpec = new SecretKeySpec(keyBytes, "AES");
//		    IvParameterSpec ivSpec = new IvParameterSpec(keyBytes);
//	        cipher.init(Cipher.DECRYPT_MODE,keySpec,ivSpec);
//			
//		    BASE64Decoder decoder = new BASE64Decoder();
//		    byte [] results = cipher.doFinal(decoder.decodeBuffer(text));
//		    return new String(results,"UTF-8");
//	    }
//    }
// 


#endregion

