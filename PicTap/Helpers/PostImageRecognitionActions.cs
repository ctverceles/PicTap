﻿using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Contacts;
using Foundation;
using UIKit;

namespace PicTap
{
	public static class PostImageRecognitionActions
	{
		static string openin = "Export";
		static string saveto = "Save to Contacts";
		static string copyto = "Copy For Pasting";

		public static async void OpenIn(CNMutableContact contact, string textClipboard = "",
		                                UIViewController VCForUIThread = null) 
		{
			var result = await UserDialogs.Instance.ActionSheetAsync(
				string.Format("What do we do with contact {0} {1}", contact.GivenName, contact.FamilyName), null, 
				null, null, 
				(string.IsNullOrWhiteSpace(textClipboard) ? new string[] { saveto, openin } : 
				 new string[] { saveto, openin, copyto})

			);

			if (string.Equals(result, openin))
			{
				DeviceUtil.Share(CombineContactDataForExporting(contact), 
				                 GlobalVariables.VCToInvokeOnMainThread); 
				/*if (!Settings.IsPremiumSettings)
				{
					Console.WriteLine("Not premium, showing interstitial");
					AdFactory.ShowInterstitial();
				}*/
			}
			else if (string.Equals(result, saveto))
			{
				ContactsHelper.PushNewContactDialogue(contact);
			}else if (string.Equals(result, copyto))
			{
				DeviceUtil.CopyToClipboard(textClipboard);
				await UserDialogs.Instance.AlertAsync("Copied!", null, "OK");
				if (!Settings.IsPremiumSettings)
				{
					Console.WriteLine("Not premium, showing interstitial");
					AdFactory.ShowInterstitial();
				}
			}
		}

		static string CNPhoneNumbersToStrings(CNLabeledValue<CNPhoneNumber>[] numbers)
		{
			string result = "Contact Numbers\n";
			for (int c = 0; c < numbers.Length; c++) {
				result += "\t"+ numbers[c].Label+ ": " + numbers[c].Value.StringValue + "\n\n";
			}
			return result;
		}

		static string CNEmailsToStrings(CNLabeledValue<NSString>[] emails)
		{
			string result = "\n";
			for (int c = 0; c < emails.Length; c++)
			{
				result += "\t" + emails[c].Label + ": " + emails[c].Value + "\n\n";
			}
			return result;
		}

		static string CombineContactDataForExporting(CNMutableContact contact) 
		{
			return string.Format("Name: {0} {1}\n{2}Organization: {3}\nEmail Addresses:{4}", 
			                     contact.GivenName, contact.FamilyName, CNPhoneNumbersToStrings(contact.PhoneNumbers), 
			                     contact.OrganizationName, CNEmailsToStrings(contact.EmailAddresses));
		}
	}
}

