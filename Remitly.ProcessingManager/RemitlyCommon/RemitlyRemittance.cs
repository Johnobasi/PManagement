namespace Remitly.ProcessingManager.RemitlyCommon
{
    public class RemitlyRemittance
	{
		public string Reference_Number { get; set; }
		public string Created_On { get; set; }
		public string State { get; set; }
		public string[] Payer_Codes { get; set; }
		public string Type { get; set; }
		public Send_Amount Send_Amount { get; set; }
		public Receive_Amount Receive_Amount { get; set; }
		public Bank_Account Bank_Account { get; set; }
		public Sender Sender { get; set; }
		public Receiver Receiver { get; set; }
        public Name Name { get; set; }
        public Address Address { get; set; }
        public Identification Identification { get; set; }

    }

	public class Send_Amount
	{
		public string Currency { get; set; }
		public decimal Amount { get; set; }
		public decimal? ExchangeRate { get; set; }
	}
	public class Receive_Amount
	{
		public string Currency { get; set; }
		public decimal Amount { get; set; }
	}
	public class Bank_Account
	{
		public string Routing_Number { get; set; }
		public string Account_Number { get; set; }
		public string Type { get; set; }
	}
	public class Sender
	{
		public Name Name { get; set; }
		public string Date_of_Birth { get; set; }
		public Address Address { get; set; }
		public string Phone { get; set; }
		public Identification[] Identification { get; set; }
		public string Source_Of_Funds { get; set; }
		public string Occupation { get; set; }
		public string Beneficiary_Relationship { get; set; }
		public string Transfer_Reason { get; set; }
	}

	public class Receiver
	{
		public Name Name { get; set; }
		public Address Address { get; set; }
		public string Phone { get; set; }
		public Identification[] Identification { get; set; }
	}

	public class Name
	{
		public string First { get; set; }
		public string Middle { get; set; }
		public string Last { get; set; }
		public string Second_Last { get; set; }
	}
	public class Address
	{
		public string[] Lines { get; set; }
		public string District { get; set; }
		public string City { get; set; }
		public string Subdivision { get; set; }
		public string Country { get; set; }
		public string Postcode { get; set; }
	}
	public class Identification
	{
		public string Type { get; set; }
		public string Number { get; set; }
	}



}
