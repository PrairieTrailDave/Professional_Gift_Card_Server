
Professional Gift Card Server

Why call this system a professional server? Because this system is a reduced function version 
of a gift card server that is running on several commercial sites. You may also notice both 
architectural differences as well as functional differences between this system and the 
other systems on this source code site.

This server has:
     Runs on Azure Database as a Service
     PCI level Encryption of important data in the database – so that when (not if) the database
	 is hacked, the hacker can’t use the information. 

          Encryption of Card numbers
          Encryption of important cardholder data
          Encryption of important merchant data

Gift Card Functions Supported:
     Receive cards into inventory
     Ship cards to merchant
     Gift Activate (put value on card)
     Gift Sale (redeem of value)
     Support for “Split Tender” on the sale
     Gift Return (put value on card)
     End of Day Summary Report
     End of Day Detail Report

Management Screens
     Create Merchant
     Edit Merchant
     Allow the merchant to edit some data
	 Pricing allows for per transaction pricing
	 Merchant Billing

Merchants can manage clerk sign ons
     
It logs everything in multiple formats so that errors can be tracked down and monitored
	All log in attempts
	All successful transactions
	All failed transactions
	All execution errors

Three tier architecture with controllers instantiating business objects and 
calling methods which call Data Access Objects to manipulate the database.

SQL Stored procedures for the transactions
SQL unit tests

We got started on Internationalization by a number of the error messages are stored in resource files
Supporting terminal ID’s for multi-lane stores
Supporting multiple time zones through merchant local time stamp 

Will there be bugs in this system? I would expect so. All we did to generate this code base was 
to take our existing system, pull out a lot of stuff, and make sure that it compiled. We did 
not run any real testing on the results to see if we caught all the bugs. I’m sure that 
something got messed up in this process.
People may also object to this using the old forms authentication. However, that is solid.



While this is a reduced functionality system, we offer a full function system 
that can be customized. We believe that purchasing the full function system 
from us will be cheaper than you attempting to put these functions back into 
this reduced system. We can also add other custom functions to this system.


What does our commercial version have that is not in the reduced version?
Gift Card Functions supported in commercial version but not the reduced version:
     Card Top Up
     Card Balance Inquiry
     Transaction Void
     Balance Transfers
     Support for chains
     Support for merchant groups such as a mall
     Support for Agents and Sub Agents who are reselling the service
     Support for Agents to have their own “BIN” range and they can issue cards to merchants
     Merchant to merchant transfers at end of day and reporting on those transfers
     IP address tracking on every transaction
     Web API designed to work with VeriFone or other POS terminals (XML responses)
     Web API designed to work with tablets or other devices (JSON responses)
     Raw socket communications (such as VISA K or Paymentech or other format that you have documentation on) 
     that some POS terminals need. 

Loyalty functions
	Loyalty Register Cardholder
	Loyalty log Sale Transaction
		With possible rewards issued on this sale
		With possible prizes awarded on this sale
		With possible coupon granted on this sale
	Loyalty Return Transaction
	Loyalty Balance Inquiry
		With possible redeem of reward
	Loyalty Management screens
		Multiple ways that you can define how points are awarded
		Multiple ways that rewards can be defined and awarded
		Multiple ways that prizes can be awarded
		Coupon management
Loyalty Reports
End of Day reports – summary and detail
Customer Ranking Report
Differential Ranking Report
Loyalty Analytics
	Revenue, Frequency, Monetary (RFM) Ranking and Reporting
	RFM Charting
	Coupon response reporting
Logging of the errors to different tables in the database instead of text files so that a 
dashboard could be written to monitor activity in real time. 

By the way, the encryption keys and initial vectors are different between this product 
and our commercial product. So, the publishing of this code does not endanger any existing 
or future systems.


Why do this? Because we have had inquiries from places that can’t afford American professional 
level work. And we hope to get people who look at this and want something customized from 
either this or our full commercial system.

Prairie Trail Software, Inc
Plano, TX
972-618-4199
