﻿
/*
Copyright 2021 Emma Kemppainen, Jesse Huttunen, Tanja Kultala, Niklas Arjasmaa
          2022 Pauliina Pihlajaniemi, Viola Niemi, Niina Nikki, Juho Tyni, Aino Reinikainen, Essi Kinnunen

This file is part of "Juttunurkka".

Juttunurkka is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, version 3 of the License.

Juttunurkka is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Juttunurkka.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace Prototype
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AktiviteettiäänestysEka : ContentPage
	{

		private int _countSeconds = 10;


		public IList<CollectionItem> Items { get; set; }
		public class CollectionItem
		{
			public int ID;
			public string ImageSource { get; set; }
			public IList<string> ActivityChoises { get; set; }
			public string Selected { get; set; }



			public CollectionItem(int ID, string ImageSource, IList<string> ActivityChoises)
			{
				this.ID = ID;
				this.ImageSource = ImageSource;
				this.ActivityChoises = ActivityChoises;
				this.Selected = null;
			}
		}

		public AktiviteettiäänestysEka()
		{
			NavigationPage.SetHasBackButton(this, false);
			InitializeComponent();

			//alustus
			Items = new List<CollectionItem>();
			string img;

			Console.WriteLine("Setting vote 1 page content");
			foreach (var item in Main.GetInstance().client.voteCandidates1)
			{
				Console.WriteLine("Key: {0}, Value: {1}", item.Key, item.Value);
				img = "emoji" + item.Key.ToString() + ".png";
				Items.Add(new CollectionItem(item.Key, img, item.Value));
			}

			BindingContext = this;

			Vote1();
		}

		private async void Vote1()
		{

			_countSeconds = Main.GetInstance().client.vote1Time;
			// TODO Xamarin.Forms.Device.StartTimer is no longer supported. Use Microsoft.Maui.Dispatching.DispatcherExtensions.StartTimer instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
			Device.StartTimer(TimeSpan.FromSeconds(1), () =>
		   {
			   _countSeconds--;

			   timer.Text = _countSeconds.ToString();


			   if (_countSeconds == 0)
			   {

				   return false;

			   }

			   return Convert.ToBoolean(_countSeconds);
		   });

			await Task.Delay(Main.GetInstance().client.vote1Time * 1000);

			SendVote1();
			bool success = await Main.GetInstance().client.ReceiveVote2Candidates();
			if (success)
			{
				//received vote 2 changing view
				await Navigation.PushAsync(new AktiviteettiäänestysToka());
			}
		}

		//Device back button disabled
		protected override bool OnBackButtonPressed()
		{
			return true;

		}


		void btnPopupButton_Clicked(object sender, EventArgs e)
		{
			
			if (sender is Button b && b.Parent is Microsoft.Maui.Controls.Compatibility.Grid g && g.Children[2] is Frame f)
			{

				if (f.IsVisible == false)
				{

					f.IsVisible = true;

				}

				else if (f.IsVisible == true)
				{

					f.IsVisible = false;
				}

				// change the text of the button to the answer
				CollectionView view = (f.Children[0] as Microsoft.Maui.Controls.Compatibility.StackLayout).Children[0] as CollectionView;
				b.Text = view.SelectedItem as string;
			}
		}




		async void SendVote1()
		{
			//prepare answer to host
			Dictionary<int, string> answer = new Dictionary<int, string>();
			foreach (var item in Items)
			{
				if (item.Selected == null)
				{
					break;
				}
				answer.Add(item.ID, item.Selected);
			}
			//avoid sending empty answer
			if (answer.Count != 0)
			{
				await Main.GetInstance().client.SendVote1Result(answer);
			}
		}
	}
}