using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace WpfThreePiles
{
   
    public partial class MainWindow : Window
    {
        List<int> initialState = new List<int> { 1, 2, 3 }; //Spēles sākuma stāvoklis. Sarakstā ir trīs cipari, kas atspoguļo akmeņu skaitu uz kociņiem.
                                                            //Nedaudz modificējot kodu un vizuālo interfisu akmeņu skaitu un spēles grūtības līmeni var izveidot dinamisku
        List<int> removedStones = new List<int> { 0, 0, 0 }; // Šajā sarakstā glabājās jau noņemto akmeņu skaits. Tas ir izveidots lai izņemt akmeņus no kociņiem lietotāja interfeisā
        int pileIndex;                                      // Glabā tekošā kociņa indeksu
        int stonesToRemove;                                 // Glabā izvēli par noņēmamo akmeņu skaitu
        bool playerATurn = true;                            // Glabā spēlētāja gājiena stāvokli
        public MainWindow()
        {
            InitializeComponent();
        }


        private void Stone_select(object sender, MouseButtonEventArgs e) //funkcija kas veic akmeņu stāvokļa izmaiņu lietotāja interfeisā
        {
            DependencyObject dpobj = sender as DependencyObject;            
            string name = dpobj.GetValue(FrameworkElement.NameProperty) as string;

            var selectedImg = (Image)this.FindName(name);
            selectedImg.Visibility = Visibility.Hidden;
            

            if (name.Contains("Selected"))
            {
                name = name.Split("Selected")[0];
            }
            else
            {
                var pi_sc = name.Substring(5).Split("_");
                pileIndex = int.Parse(pi_sc[0]);
                stonesToRemove = int.Parse(pi_sc[1]) - removedStones[pileIndex - 1];
                //MessageBox.Show("Pile " + pileIndex + " Stone " + stonesToRemove);
                name = name + "Selected";
                
            }
            selectedImg = (Image)this.FindName(name);
            selectedImg.Visibility = Visibility.Visible;

        }

        private List<int> FindBestMove(List<int> state, bool isMaximizingPlayer) //funkcija labākā gājiena meklēšanai
        {       
            List<int> bestMove = null;
            int bestValue = isMaximizingPlayer ? int.MinValue : int.MaxValue; //piešķir sākuma vērtību labākam gājienam: ja Max-spelētājs, tad piešķir 
                                                                              //mazāku iespējamu vērtību un kāpj uz augšu meklējot pēc iespējas labāku vērtību.
                                                                              //ja Min spēlētājs, tad otrādi.

            for (int pile = 0; pile < 3; pile++)                              //iterācija pa visiem pieejamajiem kociņiem
            {
                for (int stones = 1; stones <= state[pile]; stones++)         //iterācija pa visiem uz kociņiem esošajiem akmeņiem
                {
                    List<int> nextState = new List<int>(state);               //izveido jaunu mainīgo, kas glabā esošo spēles stāvokli
                    nextState[pile] -= stones;                                //izmaina spēles stāvokli atņemot no esošā akmeņu skaita uz kociņa cikla stones tekošo vērtību
                    int value = Minimax(nextState, !isMaximizingPlayer);      //izsauc rekursīvu Minimax funkciju vienlaicīgi mainot nākamā līmeņa spelētāja mērķi (Min vai Max),
                                                                              //kas pārvietojoties pa nextState koku atrod vislabāku iespējamo vērtību
                    if (isMaximizingPlayer && value > bestValue || !isMaximizingPlayer && value < bestValue) //Pārbaude, vai iegūtais labākais stāvoklis ir labāks par tekošo
                    {
                        bestValue = value;
                        bestMove = nextState;                                  //ja stāvoklis ir labāks, saglabā atgiežamā sarakstā
                    }
                }
            }

            return bestMove;
        }

        static int Minimax(List<int> state, bool isMaximizingPlayer)         //rekursīvā MiniMax funkcija, kas ģenerē visu iesējamo stāvokļu koku vienlaicīgi vērtājot ceļu
        {
            if (state[0] == 0 && state[1] == 0 && state[2] == 0)
            {
                return isMaximizingPlayer ? -1 : 1;                         //ja sasniegta koka lapa/strupceļš, veic novērtējumu par esošo poziciju atgriežot -1, ja Max spelētājs
            }

            int bestValue = isMaximizingPlayer ? int.MinValue : int.MaxValue;

            for (int pile = 0; pile < 3; pile++)
            {
                for (int stones = 1; stones <= state[pile]; stones++)
                {
                    List<int> nextState = new List<int>(state);
                    nextState[pile] -= stones;
                    int value = Minimax(nextState, !isMaximizingPlayer);    //funkcijas rekursija
                    bestValue = isMaximizingPlayer ? Math.Max(bestValue, value) : Math.Min(bestValue, value);
                }
            }

            return bestValue;                                               //atgriež ceļa labāku vertējumu pēc koka zara apgaitas
        }

       


        private void btn_Remove_Click(object sender, RoutedEventArgs e)     //akmeņa noņemšanas pogas funkcija
        {
            chkBox_PlayerTurn.IsEnabled = false;
            RemoveStones();                                                 //funkcija jas noņem atbilstošu skaitu akmeņu no kociņa lietotāja interfeisā

            if (!IsGameOver())                                              //pārbaude, vai pēc spelētāja gājiena ir vēl palikuši akmeņi
            {
                PlayerBMove(chkBox_PlayerTurn.IsChecked == false ? true : false); //ja ir izsauc datora gājienu kā argumentu pasniedzot funkcijai dators spēlē Max vai Min
            }
            

        }

        private void PlayerBMove(bool ifMaxPlayer)                          //datora gājiens
        {
            var bestMove = FindBestMove(initialState, ifMaxPlayer);         //meklē labāku stāvokli
            for (int i = 0; i <= 2; i++)                                    //atrod stāvokļa izmaiņas, lai veiktu atbilstošas izmaiņas lietotāja interfeisā
            {
                if (bestMove[i] != initialState[i])
                {
                    pileIndex = i + 1; break;                               
                }
            }
            stonesToRemove = initialState[pileIndex - 1] - bestMove[pileIndex - 1];
            
            RemoveStones();                                                 //veic izmaiņas lietotāja interfeisā
        }

        public void RemoveStones()
        {
            Image selectedImg;
            for (int i=1; i <= stonesToRemove;  i++)                        //meklē attēlus ko paslēpt (noņemt no kociņa) lietotāja interfeisā
            {
                string name = "Stone" + (pileIndex) + "_" + (removedStones[pileIndex-1]+i);
                selectedImg = (Image)this.FindName(name);
                selectedImg.Visibility = Visibility.Hidden;
                name = name + "Selected";
                selectedImg = (Image)this.FindName(name);
                selectedImg.Visibility = Visibility.Hidden;
            }
            var myMove = new List<int> {pileIndex == 1 ? initialState[0] - stonesToRemove : initialState[0],
                                      pileIndex == 2 ? initialState[1] - stonesToRemove : initialState[1],
                                      pileIndex == 3 ? initialState[2] - stonesToRemove : initialState[2] };
            initialState = myMove;                                          //pēc izmaiņu pielietošanas izminā sākuma stāvokli uz tekošo
            removedStones[pileIndex-1] = stonesToRemove;
            playerATurn = !playerATurn;                                     //izmaina spelētāja kārtu (A↔B)

            if (IsGameOver())                                               //pārbaude vai spēle ir beigusies un ja tā, paziņo rezultātu
            {
                btn_Remove.IsEnabled = false;
                clearAll();
                MessageBox.Show(($"Player {(playerATurn ? "B (Computer)" : "A (You)")} wins!"));
            }
            
        }

        private bool IsGameOver()                                          //spēles beigu nosacījuma pārbaude
        {
            return initialState[0] <= 0 && initialState[1] <= 0 && initialState[2] <= 0;            
        }
        private void btn_Reset_Click(object sender, RoutedEventArgs e)      // pogas Reset notikuma apstrāde
        {
            chkBox_PlayerTurn.IsEnabled = true;
            Reset();
        }
        private void Reset()                                               // spēles stāvokļa atjaunināšana
        {
            Image selectedImg;                  
            for (int i = 1; i <= 3; i++)                                   // attēlu lietotāja interfeisā atjaunināšana
            {
                for (int j = 1; j <= i; j++)
                {
                    string name = "Stone" + (i) + "_" + (j);
                    selectedImg = (Image)this.FindName(name);
                    selectedImg.Visibility = Visibility.Visible;

                }
            }
            initialState = new List<int> { 1, 2, 3 };                   
            removedStones = new List<int> { 0, 0, 0 };
            btn_Remove.IsEnabled = true;
            playerATurn = chkBox_PlayerTurn.IsChecked==true?true:false;
            if (chkBox_PlayerTurn.IsChecked == false)
            {
                PlayerBMove(true);
            }
        }

        
        private void chkBox_PlayerTurn_Checked(object sender, RoutedEventArgs e) //spēli sāk spelētājs
        {
            Reset();
            playerATurn = true;
        }

        private void chkBox_PlayerTurn_Unchecked(object sender, RoutedEventArgs e) //spēli sāk dators
        {
            playerATurn = false;
            Reset();
            
            
        }
        
        private void clearAll()
        {
            Image selectedImg;
            for (int i = 1; i <= 3; i++)
            {
                for (int j = 1; j <= i; j++)
                {
                    string name = "Stone" + (i) + "_" + (j);
                    selectedImg = (Image)this.FindName(name);
                    selectedImg.Visibility = Visibility.Hidden;

                }
            }
        }
        
    }
}
