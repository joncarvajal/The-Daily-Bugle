﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheDailyBugle.Services;
using Xamarin.Forms;
using TheDailyBugle.Models;
using System.Data;
using Dapper;
using System.Data.SqlClient;
using Xamarin.Forms.Xaml;
using System.Diagnostics;

namespace TheDailyBugle
{
	public partial class TitlePage : ContentPage
	{

        private readonly IEnumerable<ComicTitle> comicTitles;
        private List<ComicTitle> subscribedComicTitles;

        public TitlePage ()
		{
			InitializeComponent ();

            hideComicsButton.IsVisible = false;
            comicsTitles.IsVisible = false;

            List<Subscription> subscriptions;
            using (IDbConnection source = new SqlConnection("Server=tcp:thedailybugle.database.windows.net,1433;Initial Catalog=The Daily Bugle;Persist Security Info=False;User ID=dbadmin;Password=1231!#ASDF!a;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
            {
                source.Open();

                // get user subscriptions
                subscriptions = source.Query<Subscription>(
                    Subscription.Select())
                    .Where(s => s.UserId.Equals(8) && s.IsActive)
                    .Distinct()
                    .ToList();

                // get all comics
                comicTitles = source.Query<ComicTitle>(
                    ComicTitle.Select())
                    .Distinct()
                    .OrderBy(ct => ct.Name)
                    .ToList();

                // get subscribed comics

                subscribedComicTitles = comicTitles
                    .Where(ct => subscriptions.Any(s => s.ComicTitleId == ct.ComicTitleId))
                    .Distinct()
                    .ToList();
            }
            
            UpdateDataBinding();
        }

        void ToggleSettings(object sender, EventArgs args)
        {
            comicsTitles.IsVisible = !comicsTitles.IsVisible;
            addComicsButton.IsVisible = !addComicsButton.IsVisible;
            hideComicsButton.IsVisible = !hideComicsButton.IsVisible;
            subscribredComicTitles.IsVisible = !subscribredComicTitles.IsVisible;
        }

        void OnDeleteClicked(object sender, EventArgs args)
        {
            var button = sender as Button;
            var comicTitle = button.Parent.BindingContext as ComicTitle;
            
            using (IDbConnection source = new SqlConnection("Server=tcp:thedailybugle.database.windows.net,1433;Initial Catalog=The Daily Bugle;Persist Security Info=False;User ID=dbadmin;Password=1231!#ASDF!a;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
            {
                source.Open();

                var subscription = source.Query<Subscription>(Subscription.Select())
                    .FirstOrDefault(s => s.ComicTitleId.Equals(comicTitle.ComicTitleId) &&
                                         s.UserId.Equals(8) &&
                                         s.IsActive);

                // delete the subscription
                subscription.Update(source, false);

                // remove from subscription list
                subscribedComicTitles.Remove(comicTitle);
                UpdateDataBinding();
            }
        }

        public void DisplayCommic(object sender, ItemTappedEventArgs e)
        {
            //go to comic page
            var comicTitle = e.Item as ComicTitle;
            Navigation.PushAsync(new ComicPage(comicTitle));
        }

        void OnUnsubbedComicTapped(object sender, ItemTappedEventArgs e)
        {
            var comicTitle = e.Item as ComicTitle; 

            var subscription = new Subscription
            {
                UserId = 8,//(int)Application.Current.Properties["userId"],
                IsActive = true,
                ComicTitleId = comicTitle.ComicTitleId
            };

            using (IDbConnection source = new SqlConnection("Server=tcp:thedailybugle.database.windows.net,1433;Initial Catalog=The Daily Bugle;Persist Security Info=False;User ID=dbadmin;Password=1231!#ASDF!a;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
            {
                // assign the subscriptionId to the primary key that will be returned
                subscription.SubscriptionId = subscription.Insert(source);
            }

            ((ListView)sender).SelectedItem = null; // de-select the row
            
            var newTitle = comicTitles
                .FirstOrDefault(ct => ct.ComicTitleId.Equals(subscription.ComicTitleId));

            subscribedComicTitles.Add(newTitle);
            UpdateDataBinding();

            Device.BeginInvokeOnMainThread(() => {
                DisplayAlert("Success", $"{newTitle.Name} has been added!" , "OK");
            });
            //subscriptions = subscriptions.OrderBy(s => s.)

            //hide unsubbed comics
            //unhide add comic button
            //unhide subbed comics?
        }

        private void UpdateDataBinding()
        {
            subscribredComicTitles.ItemsSource = subscribedComicTitles;

            comicsTitles.ItemsSource = comicTitles
                .Where(ct => !subscribedComicTitles.Any(s => s.ComicTitleId == ct.ComicTitleId));
        }

        private List<Subscription> GetUserSubscriptions(int userId)
        {
            using (IDbConnection source = new SqlConnection("Server=tcp:thedailybugle.database.windows.net,1433;Initial Catalog=The Daily Bugle;Persist Security Info=False;User ID=dbadmin;Password=1231!#ASDF!a;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
            {
                source.Open();
                var subscriptions = source.Query<Subscription>(
                    Subscription.Select())
                    .Where(s => s.UserId.Equals(userId))
                    .ToList();

                return subscriptions;
            }
        }

        private User CreateUser(string email)
        {
            using (IDbConnection target = new SqlConnection("Server=tcp:thedailybugle.database.windows.net,1433;Initial Catalog=The Daily Bugle;Persist Security Info=False;User ID=dbadmin;Password=1231!#ASDF!a;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
            {
                var user = new User
                {
                    Email = email
                };

                target.Open();
                user.Insert(target);

                return user;
            }
        }
    }
}