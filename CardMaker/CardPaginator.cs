using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CardMaker
{
    public class CardPaginator : DocumentPaginator
    {
        private const double CardMarginLeft = 6.0;
        private const double CardMarginTop = 6.0;
        private const double DPI = 96.0;
        private int CardsPerRow;
        private int CardsPerColumn;
        private int CardsPerPage;

        private int _PageCount;
        private Size _CardSize;
        private Size _PageSize;
        private List<CardModelView> _Cards;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cards"></param>
        /// <param name="cardSize"></param>
        /// <param name="pageSize"></param>
        public CardPaginator(List<CardModelView> cards, Size cardSize, Size pageSize)
        {
            double cardsPerRow = (pageSize.Width / cardSize.Width);
            double cardsPerColumn = (pageSize.Height / cardSize.Height);

            CardsPerRow = (int)Math.Floor(cardsPerRow);
            CardsPerColumn = (int)Math.Floor(cardsPerColumn);
            CardsPerPage = CardsPerRow * CardsPerColumn;
            
            _PageCount = (int)Math.Ceiling((double)((double)cards.Count / (double)CardsPerPage));
            _CardSize = cardSize;
            _Cards = cards;
            
        }

        public override DocumentPage GetPage(int pageNumber)
        {
            //first, calculate which card starts on this page
            int startIndex = (pageNumber) * CardsPerPage;
            int endCount = CardsPerPage;
            if (startIndex + CardsPerPage > _Cards.Count) endCount = _Cards.Count % CardsPerPage;

            //MessageBox.Show(
            //    "Total Pages " + _PageCount +
            //    "\nCards Per Page " + CardsPerPage +
            //    "\nCurrent Page No. " + pageNumber +
            //    "\nStart Index " + startIndex +
            //    "\nEnd Count " + endCount +
            //    "\nCard Count " + _Cards.Count );
            
            List<CardModelView> sub = _Cards.GetRange(startIndex, endCount);
            
            return new DocumentPage(BuildPage(startIndex, endCount));
        }

        /// <summary>
        /// Sets up a fixed page that contains the appropriately
        /// indexed cards, positioned and laid out using UIElements.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endCount"></param>
        /// <returns></returns>
        private UIElement BuildPage(int startIndex, int endCount)
        {
            Grid page = new Grid();
            page.Margin = new Thickness(18); //quarter-inch, also counting the per-card margin applied below
            page.Background = Brushes.White;
            page.Width = _PageSize.Width;
            page.Height = _PageSize.Height;

            //setup all cards on the page. Position
            //them relative to their per-page index.
            int cardColumn = 0;
            int cardRow = 0;
            for (int i = startIndex; i < startIndex + endCount; i++, cardColumn++)
            {
                if (cardColumn >= CardsPerRow)
                {
                    cardColumn = 0;
                    cardRow++;
                }

                //setup card layout
                Canvas canvas = new Canvas();
                //canvas.Margin = new Thickness(3);
                TextBlock tbTitle = new TextBlock();
                tbTitle.Text = "blank";
                tbTitle.FontSize = 16;
                tbTitle.FontFamily = new FontFamily("Arial");
                tbTitle.Margin = new Thickness(16,16,0,0);

                TextBlock tbTypeAndLevel = new TextBlock();
                tbTypeAndLevel.FontSize = 10;
                tbTypeAndLevel.FontFamily = new FontFamily("Arial");
                tbTypeAndLevel.Margin = new Thickness(25,35,0,0);

                TextBlock tbAttackDef = new TextBlock();
                tbAttackDef.FontSize = 16;
                tbAttackDef.FontFamily = new FontFamily("Arial");
                tbAttackDef.Margin = new Thickness(1.5*DPI, 3.0*DPI, 0.1*DPI, 0);
                tbAttackDef.HorizontalAlignment = HorizontalAlignment.Right;

                TextBlock tbDesc = new TextBlock();
                tbDesc.FontSize = 10;
                tbDesc.FontFamily = new FontFamily("Arial");
                tbDesc.Margin = new Thickness(0.25 * DPI, 1.5*DPI, 0, 0);
                tbDesc.Width = 2.0 * DPI;
                tbDesc.Height = 2.0 * DPI;
                tbDesc.TextWrapping = TextWrapping.Wrap;

                Border border = new Border();
                border.BorderThickness = new Thickness(1);
                border.BorderBrush = Brushes.Black;
                border.Width = _CardSize.Width;
                border.Height = _CardSize.Height;

                canvas.Children.Add(tbTitle);
                canvas.Children.Add(tbTypeAndLevel);
                canvas.Children.Add(tbAttackDef);
                canvas.Children.Add(tbDesc);
                canvas.Children.Add(border);

                //set card position (DPI is already applied)
                //hack positioning by using margins
                canvas.Margin = new Thickness(
                    _CardSize.Width * cardColumn + CardMarginLeft,
                    _CardSize.Height * cardRow + CardMarginTop,
                    0.0, 
                    0.0 );
                page.Children.Add((UIElement)canvas);

                //fill in card data
                tbTitle.Text = _Cards[i].Name;
                tbTypeAndLevel.Text = _Cards[i].Affiliation + " " + _Cards[i].CardType;
                if (_Cards[i].CardType == "Ally" || _Cards[i].CardType == "Enhancement")
                {
                    tbTypeAndLevel.Text += "   Level " +_Cards[i].Level;
                }
                if (_Cards[i].CardType == "Ally")
                {
                    tbAttackDef.Text =  _Cards[i].Attack + "/" + _Cards[i].Defense;
                }
                else if (_Cards[i].CardType == "Objective")
                {
                    tbAttackDef.Text = "HP: " + _Cards[i].Defense;
                }
                tbDesc.Text = _Cards[i].Description;
            }

            page.Measure(_PageSize);
            page.Arrange(new Rect(new Point(0, 0), _PageSize));
            page.UpdateLayout();
            
            return page;
        }

        public override bool IsPageCountValid
        {
            get { return true; }
        }

        public override int PageCount
        {
            get { return _PageCount; }
        }

        public override System.Windows.Size PageSize
        {
            get { return _PageSize; }
            set
            {
                _PageSize = value;
            }
        }

        public override IDocumentPaginatorSource Source
        {
            get { return null; }
        }
    }
}
