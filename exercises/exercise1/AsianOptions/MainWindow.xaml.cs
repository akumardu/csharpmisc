using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

using System.Threading.Tasks;


namespace AsianOptions
{

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		//
		// Methods:
		//
		public MainWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Exit the app.
		/// </summary>
		private void mnuFileExit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();  // trigger "closed" event as if user had hit "X" on window:
		}

		/// <summary>
		/// Saves the contents of the list box.
		/// </summary>
		private void mnuFileSave_Click(object sender, RoutedEventArgs e)
		{
			using (StreamWriter file = new StreamWriter("results.txt"))
			{
				foreach (string item in this.lstPrices.Items)
					file.WriteLine(item);
			}
		}

	    private int counter = 0;

		/// <summary>
		/// Main button to run the simulation.
		/// </summary>
		private void cmdPriceOption_Click(object sender, RoutedEventArgs e)
		{
			//this.cmdPriceOption.IsEnabled = false;
			
			//this.SpinnerWait.Visibility = System.Windows.Visibility.Visible;
		    this.Working.Visibility = System.Windows.Visibility.Visible;
		    counter++;
		    this.lstCount.Content = counter.ToString();

			double initial = Convert.ToDouble(txtInitialPrice.Text);
			double exercise = Convert.ToDouble(txtExercisePrice.Text);
			double up = Convert.ToDouble(txtUpGrowth.Text);
			double down = Convert.ToDouble(txtDownGrowth.Text);
			double interest = Convert.ToDouble(txtInterestRate.Text);
			long periods = Convert.ToInt64(txtPeriods.Text);
			long sims = Convert.ToInt64(txtSimulations.Text);

		    string result = "";

            //
            // Run simulation to price option:
            //

            Task computationTask = new Task(() =>
		    {
		        Random rand = new Random();
		        int start = System.Environment.TickCount;

		        double price = AsianOptionsPricing.Simulation(rand, initial, exercise, up, down, interest, periods, sims);

		        int stop = System.Environment.TickCount;

		        double elapsedTimeInSecs = (stop - start) / 1000.0;

		        result = string.Format("{0:C}  [{1:#,##0.00} secs]",
		                                      price, elapsedTimeInSecs);
		    });

		        //
		        // Display the results:
		        //
            Task updateTask = computationTask.ContinueWith((antecedent) =>
            { 
		        this.lstPrices.Items.Insert(0, result);

		        //this.SpinnerWait.Spin = false;
		        //this.SpinnerWait.Visibility = System.Windows.Visibility.Collapsed;

                //this.cmdPriceOption.IsEnabled = true;
                counter--;
                this.lstCount.Content = counter.ToString();
                if (counter == 0)
                {
                    this.Working.Visibility = System.Windows.Visibility.Collapsed;
                }
            },

            // the FromCurrentSync below means that all UI stuff executes in the
            // same thread (UI) context and since there is only one UI thread
            // it is going to incr/decr the counter variable
            // and hence there will be no conflict
            TaskScheduler.FromCurrentSynchronizationContext()
            );

            computationTask.Start();

            AsianOptionsPricing.TestStandardoutput();
            
		}

	}//class
}//namespace
