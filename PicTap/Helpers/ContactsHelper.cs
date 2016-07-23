﻿using System;
using Acr.UserDialogs;
using AddressBook;
using Contacts;
using ContactsUI;
using Foundation;
using UIKit;

namespace PicTap
{
	public class ContactsHelper
	{
		ABAddressBook abb = new ABAddressBook();
		ABPerson[] contacts = null;

		public ContactsHelper()
		{
			Console.WriteLine("Storing all contacts in iOS memory");
			contacts = abb.GetPeople();
		}

		public static void PushNewContactDialogue(string firstname, string lastname, 
		     CNLabeledValue<CNPhoneNumber>[] numbers, string org) 
		{ 
			NSError error;
			var store = new CNContactStore();
			var fetchKeys = new[] { CNContactViewController.DescriptorForRequiredKeys };
			var mutableContact = new CNMutableContact { 
				GivenName = firstname,
				FamilyName = lastname,
				PhoneNumbers = numbers,
				OrganizationName = org
			};
			CNContactViewController editor;

			editor = CNContactViewController.FromNewContact(mutableContact);

			Console.WriteLine("configuring CNContactViewController");
			// Configure editor
			editor.ContactStore = store;
			editor.AllowsActions = true;
			editor.AllowsEditing = true;
			editor.Delegate = new CNViewControllerDelegate();
			Console.WriteLine("done configuring CNContactViewController");

			PushCNContactViewControllerWithToolBarItemsOutsideUINavigationController(editor, Values.APPNAME);

			Console.WriteLine("Done w function");
		}

		public static void PushCNContactViewControllerWithToolBarItemsOutsideUINavigationController(
			UIViewController controlToPush, string title = "") 
		{ 
			var navcontrol = new UINavigationController
			{
				Title = title
			};
			navcontrol.PushViewController(controlToPush, true);
			var vc = UIApplication.SharedApplication.KeyWindow.RootViewController;
			while (vc.PresentedViewController != null)
			{
				vc = vc.PresentedViewController;
			}
			vc.PresentViewController(navcontrol, true, () => { });
		}

		public static void DismissCNContactViewControllerWithToolBarItemsOutsideUINavigationController(
			bool animated, Action completionHandler = null)
		{
			iOSNavigationHelper.GetUINavigationController().DismissViewController(animated, completionHandler);
		}

		string SaveImageThenGetPath(ContactData contact, NSData image, ABPersonImageFormat format)
		{
			string filename = "";
			try
			{
				if (format == ABPersonImageFormat.Thumbnail)
				{
					filename = System.IO.Path.Combine(Environment.GetFolderPath
						(Environment.SpecialFolder.Personal),
						string.Format("{0}.jpg", contact.ID));
				}
				else {
					filename = System.IO.Path.Combine(Environment.GetFolderPath
						(Environment.SpecialFolder.Personal),
						string.Format("{0}-large.jpg", contact.ID));
				}

				image.Save(filename, true);

				Console.WriteLine("Found {0} {1}'s image. Saving it as {2}",
					contact.FirstName, contact.LastName, filename);

				return filename;
			}
			catch (Exception e)
			{
				Console.WriteLine("Error in SaveImageThenGetPath(): {0}", e.Message);
			}
			return string.Empty;
		}
		public bool SaveContactToDevice(string firstName, string lastName, string phone, string aff)
		{
			try
			{
				ABAddressBook ab = new ABAddressBook();
				ABPerson p = new ABPerson();

				p.FirstName = firstName;
				p.LastName = lastName;
				p.Organization = aff;
				//p.GetImage(ABPersonImageFormat.Thumbnail).

				ABMutableMultiValue<string> phones = new ABMutableStringMultiValue();
				phones.Add(phone, ABPersonPhoneLabel.Mobile);

				p.SetPhones(phones);

				ab.Add(p);
				ab.Save();

				UserDialogs.Instance.ShowSuccess("Contact saved: " + firstName + " " + lastName, 2000);

				return true;
			}
			catch (Exception e)
			{
				System.Console.WriteLine("[iOS.PhoneContacts] Couldn't save contact: {0} {1}, {2}", firstName, lastName, e.Message);
				UserDialogs.Instance.ShowError("Failed to save contact: " + firstName + " " + lastName + ". Pls try again.", 2000);
			}
			return false;
		}
	}
}

