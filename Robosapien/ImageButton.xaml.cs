using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Com.Enterprisecoding.Robosapien {
	public partial class ImageButton : UserControl
	{
		#region Properties
		public ImageSource Image {
			get { return (ImageSource)GetValue(ImageProperty); }
			set { SetValue(ImageProperty, value); }
		}

		public Color BlurColor {
			get { return (Color)GetValue(BlurColorProperty); }
			set { SetValue(BlurColorProperty, value); }
		} 
		#endregion

		#region Dependency Properties
		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(ImageSource), typeof(ImageButton), new UIPropertyMetadata(null));
		public static readonly DependencyProperty BlurColorProperty = DependencyProperty.Register("BlurColor", typeof(Color), typeof(ImageButton), new UIPropertyMetadata(null)); 
		#endregion

		public event RoutedEventHandler Click;

		public ImageButton() {
			BlurColor = Colors.White;
 
			this.InitializeComponent();
		}

		private void button_Click(object sender, RoutedEventArgs e)  {
			if (Click != null) {
				Click(this, e);
			}
		}
	}
}