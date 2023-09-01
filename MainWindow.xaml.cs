/*
 * 작성자: 윤정도
 * 생성일: 6/11/2023 7:40:12 PM
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MapleJaeMul
{
    enum EnchantResultState
    {
        Success,
        Failed,
        Destroyed
    }
    public partial class MainWindow : Window
    {
        public EnchantState State { get; } = new ();

        private long _leftMeso = 1000000000000;
        private long _usedMeso = 0;
        private Random _random  = new ();

        public MainWindow()
        {
            InitializeComponent();
            EnchantingItemComboBox.SelectedIndex = 0;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var scrollViewer = ScrollViewerExt.Find(LogListView);
            ScrollViewerExt.EnableMoveToBottom(scrollViewer);
        }


        void Enchant()
        {
            if (_leftMeso < State.EnchangeMeso)
            {
                MessageBox.Show("강화하는데 필요한 메소가 부족합니다.");
                return;
            }

            EnchantResultState result ;

            float destroyProb = State.DestroyProb;
            float failProb = State.FailProb;
            float rand = (float)_random.NextDouble() * 100.0f;

            if (rand < destroyProb) result = EnchantResultState.Destroyed;
            else if (rand < failProb) result = EnchantResultState.Failed;
            else result = EnchantResultState.Success;

            int nextStar = State.Star + 1;
            _usedMeso += State.EnchangeMeso;
            _leftMeso -= State.EnchangeMeso;

            if (LogListView.Items.Count >= 1000)
                LogListView.Items.RemoveAt(0);

            if (result == EnchantResultState.Success)
            {
                State.ContinousDecreaseCount = 0;
                State.Star = nextStar;
                LogListView.Items.Add($"{nextStar}성 강화성공");
            }
            else if (result == EnchantResultState.Failed)
            {
                if ((State.Star >= 16 && State.Star <= 19) || State.Star >= 21)
                {
                    State.Star -= 1;
                    ++State.ContinousDecreaseCount;
                }

                LogListView.Items.Add($"{nextStar}성 강화실패");
            }
            else
            {
                State.ContinousDecreaseCount = 0;
                State.Star = 12;
                LogListView.Items.Add($"{nextStar}성 강화실패로 장비가 파괴됨 ");
            }



            UpdateView();
        }

        void UpdateView()
        {
            if (State.Star >= 15 && State.Star <= 16)
            {
                PreventDetroyCheckBox.IsEnabled = true;
            }
            else
            {
                PreventDetroyCheckBox.IsChecked = false;
                PreventDetroyCheckBox.IsEnabled = false;
            }

            if (State.TotalDiscount <= 0.0f)
            {
                EnchantOriginalMesoTextBlock.Visibility = Visibility.Hidden;
                LastDiscountRateGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                EnchantOriginalMesoTextBlock.Visibility = Visibility.Visible;
                LastDiscountRateGrid.Visibility = Visibility.Visible;
                LastDiscountRateTextBlock.Text = $"{State.TotalDiscount * 100.0f,0:F1}%";
            }

            LeftMesoTextBlock.Text = _leftMeso.ToString("N0");
            UsedMesoTextBlock.Text = _usedMeso.ToString("N0");
            EnchantMesoTextBlock.Text = State.EnchangeMeso.ToString("N0");
            EnchantOriginalMesoTextBlock.Text = State.EnchangeOriginalMeso.ToString("N0");
            EnchantOptionTextBox.Text = State.OptionText();

            if (State.SuperCatch)
            {
                ItemBorder.BorderBrush = Brushes.Yellow;
                EnchantButton.Background = Brushes.Yellow;
                (EnchantButton.Content as TextBlock).Foreground = Brushes.Black;
            }
            else
            {
                ItemBorder.BorderBrush = Brushes.Transparent;
                EnchantButton.Background = Brushes.DarkGreen;
                (EnchantButton.Content as TextBlock).Foreground = Brushes.White;
            }


            MVPEnabledTextBlock.Visibility = State.MVPDiscount > 0.0f ? Visibility.Visible : Visibility.Collapsed;
            PCEnabledTextBlock.Visibility = State.PremiumPCDiscount > 0.0f ? Visibility.Visible : Visibility.Collapsed;
            SundayEnabledImage.Visibility = State.SundayMapleDiscount > 0.0f ? Visibility.Visible : Visibility.Collapsed;
        }

        private void InitLeftMesoButton_Click(object sender, RoutedEventArgs e)
        {
            _leftMeso = 1000000000000;
            UpdateView();
        }

        private void InitUsedMesoButton_Click(object sender, RoutedEventArgs e)
        {
            _usedMeso = 0;
            UpdateView();
        }

        private void InitStarButton_Click(object sender, RoutedEventArgs e)
        {
            State.Star = 0;
            UpdateView();
        }

        private void EnchantButton_Click(object sender, RoutedEventArgs e)
        {
            Enchant();
        }

        private void EnableSundayButton_Click(object sender, RoutedEventArgs e)
        {
            State.SundayMaple = !State.SundayMaple;
            UpdateView();
        }

        private void EnablePCButton_Click(object sender, RoutedEventArgs e)
        {
            State.PremiumPC = !State.PremiumPC;
            UpdateView();
        }

        private void MVPComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            State.MVP = (MVPClass)MVPComboBox.SelectedIndex;
            UpdateView();
        }

        private void PreventDetroyCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            State.PreventDestroy = true;
            UpdateView();
        }

        private void PreventDetroyCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            State.PreventDestroy = false;
            UpdateView();
        }

        private void EnchantingItemComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EnchantingItemComboBox.SelectedIndex == 0)
                State.Item = ItemInfo.도미네이터_펜던트;
            else if (EnchantingItemComboBox.SelectedIndex == 1)
                State.Item = ItemInfo.하이네스_워리어_헬름;
            else if (EnchantingItemComboBox.SelectedIndex == 2)
                State.Item = ItemInfo.루즈_컨트롤_머신_마크;
            else if (EnchantingItemComboBox.SelectedIndex == 3)
                State.Item = ItemInfo.아케인셰이드_아처숄더;
            else if (EnchantingItemComboBox.SelectedIndex == 4)
                State.Item = ItemInfo.에테르넬_메이지로브;
            UpdateView();
        }


        private void ChangeStarCountButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ChangeStarTextBox.Text, out var star))
            {
                MessageBox.Show("올바른 강화 수치를 입력해주세요.");
                return;
            }

            if (star > 25 || star < 0)
            {
                MessageBox.Show("올바른 강화 수치를 입력해주세요.");
                return;
            }

            State.Star = star;
            UpdateView();
        }

        private void UIElement_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
