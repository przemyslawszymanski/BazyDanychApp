using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LINQtoSQLClassDataContext dc = new LINQtoSQLClassDataContext(Properties.Settings.Default.BibliotekaDBConnectionString);
        int index_users;
        int index_return;
        int index_books;

        public MainWindow()
        {
            InitializeComponent();

            var users_list = from czytelnik in dc.Czytelnik
                             select new { czytelnik.Imie, czytelnik.Naziwsko, czytelnik.Email, czytelnik.Telefon, czytelnik.Adres };
            if (dc.DatabaseExists()) List_Users.ItemsSource = users_list;


            if (dc.DatabaseExists()) Add_Book.ItemsSource = dc.Ksiazka;

        }

        private void ButtonPopUpExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
            ButtonCloseMenu.Visibility = Visibility.Visible;
        }

        private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonOpenMenu.Visibility = Visibility.Visible;
            ButtonCloseMenu.Visibility = Visibility.Collapsed;
        }

        private void ListViewMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = ListViewMenu.SelectedIndex;
            if (index == 0)
            {
                var books_list = from ksiazka in dc.Ksiazka
                                 join autor in dc.Autor on ksiazka.Fk_Author_Id equals autor.Pk_Autor_Id
                                 join kategoria in dc.Kategoria on ksiazka.Fk_Kategoria_Id equals kategoria.Pk_Kategoria_Id
                                 select new { ksiazka.Tytul, autor.Imie, autor.Nazwisko, kategoria.Kategoria1, ksiazka.Wydawnictwo, ksiazka.Dostepnosc };
                if (dc.DatabaseExists()) List_Book.ItemsSource = books_list;

                BookList.Visibility = Visibility.Visible;
                RentBook.Visibility = Visibility.Collapsed;
                AddBook.Visibility = Visibility.Collapsed;
                USERSLIST.Visibility = Visibility.Collapsed;
            } else if (index == 1)
            {
                BookList.Visibility = Visibility.Collapsed;
                RentBook.Visibility = Visibility.Visible;
                AddBook.Visibility = Visibility.Collapsed;
                USERSLIST.Visibility = Visibility.Collapsed;
            } else if (index == 2)
            {
                BookList.Visibility = Visibility.Collapsed;
                RentBook.Visibility = Visibility.Collapsed;
                AddBook.Visibility = Visibility.Visible;
                USERSLIST.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonUser_Click(object sender, RoutedEventArgs e)
        {
            var users_list = from czytelnik in dc.Czytelnik
                             select new {czytelnik.Imie, czytelnik.Naziwsko, czytelnik.Email, czytelnik.Telefon, czytelnik.Adres };
            if (dc.DatabaseExists()) List_Users.ItemsSource = users_list;

            BookList.Visibility = Visibility.Collapsed;
            USERSLIST.Visibility = Visibility.Visible;
            RentBook.Visibility = Visibility.Collapsed;
            AddBook.Visibility = Visibility.Collapsed;
        }

        private void ButtonAddUser_Click(object sender, RoutedEventArgs e)
        {
            int last_number_ID = dc.Czytelnik.Max(u => u.Pk_Czytelnik_Id) + 1;

            string name = Name.Text;
            string surname = Surname.Text;
            string email = Email.Text;
            string phone = Phone.Text;
            string address = Address.Text;


            Czytelnik nowyCzytelnik = new Czytelnik
            {
                Pk_Czytelnik_Id = last_number_ID,
                Imie = name,
                Naziwsko = surname,
                Email = email,
                Telefon = phone,
                Adres = address
            };

            dc.Czytelnik.InsertOnSubmit(nowyCzytelnik);
            dc.SubmitChanges();
                                                        //Dodać uzytkownika do bazy (poszczególne informacje w stringach)

            

            //powrót do listy użytkowników
            var users_list = from czytelnik in dc.Czytelnik
                             select new { czytelnik.Imie, czytelnik.Naziwsko, czytelnik.Email, czytelnik.Telefon, czytelnik.Adres };
            if (dc.DatabaseExists()) List_Users.ItemsSource = users_list;

            Add_Users.Visibility = Visibility.Collapsed;
        }

        private void ButtonUser1_Click(object sender, RoutedEventArgs e)
        {
            Add_Users.Visibility = Visibility.Collapsed;
        }

        private void ButtonBook1_Click(object sender, RoutedEventArgs e)
        {
            Return_Book.Visibility = Visibility.Collapsed;
        }

        private void ButtonReturnBook_Click(object sender, RoutedEventArgs e)
        {
            //Zwracanie książki naciśniętej w zwrotach (index_return) 

            var books_to_return1 = from wypozyczenie in dc.Wypozyczenie
                                  join ksiazka in dc.Ksiazka on wypozyczenie.Fk_Ksiazka_Id equals ksiazka.Pk_Ksiazka_Id
                                  join czytelnik1 in dc.Czytelnik on wypozyczenie.Fk_Czytelnik_Id equals czytelnik1.Pk_Czytelnik_Id
                                  join autor in dc.Autor on ksiazka.Fk_Author_Id equals autor.Pk_Autor_Id
                                  where wypozyczenie.Fk_Czytelnik_Id == index_users
                                  select new { wypozyczenie.Pk_Wypozyczenie_Id, ksiazka.Tytul, autor.Imie, autor.Nazwisko, wypozyczenie.Data_oddania };

            int liczba = books_to_return1.Skip(index_return-1).First().Pk_Wypozyczenie_Id;

            Wypozyczenie wypozyczenieOBJ = dc.Wypozyczenie.SingleOrDefault(x => x.Pk_Wypozyczenie_Id == liczba);
            var book_ID = wypozyczenieOBJ.Fk_Ksiazka_Id;

            Ksiazka ksiazkaOBJ = dc.Ksiazka.SingleOrDefault(x => x.Pk_Ksiazka_Id == book_ID);
            ksiazkaOBJ.Dostepnosc = "dostepna";

            dc.Wypozyczenie.DeleteOnSubmit(wypozyczenieOBJ);

            dc.SubmitChanges();

             // zpowrót do listy ksiazek w zwrotach

            var books_to_return = from wypozyczenie in dc.Wypozyczenie
                                  join ksiazka in dc.Ksiazka on wypozyczenie.Fk_Ksiazka_Id equals ksiazka.Pk_Ksiazka_Id
                                  join czytelnik1 in dc.Czytelnik on wypozyczenie.Fk_Czytelnik_Id equals czytelnik1.Pk_Czytelnik_Id
                                  join autor in dc.Autor on ksiazka.Fk_Author_Id equals autor.Pk_Autor_Id
                                  where wypozyczenie.Fk_Czytelnik_Id == index_users
                                  select new { wypozyczenie.Pk_Wypozyczenie_Id, ksiazka.Tytul, autor.Imie, autor.Nazwisko, wypozyczenie.Data_oddania };

            if (dc.DatabaseExists()) Return1_Book.ItemsSource = books_to_return;
            Return_Book.Visibility = Visibility.Collapsed;
        }

        private void ButtonBorrow2_Click(object sender, RoutedEventArgs e)
        {
            Borrow_Book.Visibility = Visibility.Collapsed;
        }

        private void ButtonBorrow1_Click(object sender, RoutedEventArgs e)
        {
            Borrow_Book.Visibility = Visibility.Collapsed;
        }

        private void ButtonBorrowBook_Click(object sender, RoutedEventArgs e)
        {
            //Po naciśnięciu wypożycz zmiana statusu w bazie danych na wypożyczony ksiazki z listy z indeksem (index_list)///////////////////////////////////////////////////////////
            Ksiazka ksiazka = dc.Ksiazka.SingleOrDefault(x => x.Pk_Ksiazka_Id == index_books);
            ksiazka.Dostepnosc = "wypozyczona";


            int wypozyczenieId;
            int max_wypozyczenie_id;
            var isempty = dc.Wypozyczenie.FirstOrDefault();
            if(isempty == null) { wypozyczenieId = 1; }
            else
            {
                max_wypozyczenie_id = dc.Wypozyczenie.Max(u => u.Pk_Wypozyczenie_Id);
                wypozyczenieId = max_wypozyczenie_id + 1;
            }

            

            Wypozyczenie noweWypozyczenie = new Wypozyczenie
            {
                Pk_Wypozyczenie_Id = wypozyczenieId,
                Fk_Ksiazka_Id = index_books,
                Fk_Czytelnik_Id = index_users,
                Data_oddania = "2019"
            };
            dc.Wypozyczenie.InsertOnSubmit(noweWypozyczenie);


            dc.SubmitChanges();

            var books_to_return = from wypozyczenie in dc.Wypozyczenie
                                  join ksiazka1 in dc.Ksiazka on wypozyczenie.Fk_Ksiazka_Id equals ksiazka1.Pk_Ksiazka_Id
                                  join czytelnik1 in dc.Czytelnik on wypozyczenie.Fk_Czytelnik_Id equals czytelnik1.Pk_Czytelnik_Id
                                  join autor in dc.Autor on ksiazka1.Fk_Author_Id equals autor.Pk_Autor_Id
                                  where wypozyczenie.Fk_Czytelnik_Id == index_users
                                  select new { wypozyczenie.Pk_Wypozyczenie_Id, ksiazka1.Tytul, autor.Imie, autor.Nazwisko, wypozyczenie.Data_oddania };

            if (dc.DatabaseExists()) Return1_Book.ItemsSource = books_to_return;



            Borrow_Book.Visibility = Visibility.Collapsed; //powrót do listy książek
        }

        private void USERSLISTadd_Click(object sender, RoutedEventArgs e)
        {
            Add_Users.Visibility = Visibility.Visible;
        }

        
        private void List_Users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var books_list = from ksiazka in dc.Ksiazka
                             join autor in dc.Autor on ksiazka.Fk_Author_Id equals autor.Pk_Autor_Id
                             join kategoria in dc.Kategoria on ksiazka.Fk_Kategoria_Id equals kategoria.Pk_Kategoria_Id
                             select new { ksiazka.Tytul, autor.Imie, autor.Nazwisko, kategoria.Kategoria1, ksiazka.Wydawnictwo, ksiazka.Dostepnosc };
            if (dc.DatabaseExists()) List_Book.ItemsSource = books_list;

            index_users = List_Users.SelectedIndex + 1;  

            BookList.Visibility = Visibility.Visible;
            RentBook.Visibility = Visibility.Collapsed;
            AddBook.Visibility = Visibility.Collapsed;
            USERSLIST.Visibility = Visibility.Collapsed;

            Czytelnik czytelnik = dc.Czytelnik.SingleOrDefault(x => x.Pk_Czytelnik_Id == index_users);
            var name = czytelnik.Imie;
            var surname = czytelnik.Naziwsko;
            string user = name + ' ' + surname;

            string index1 = user;   
            User.Text = index1;

            var books_to_return = from wypozyczenie in dc.Wypozyczenie
                                  join ksiazka in dc.Ksiazka on wypozyczenie.Fk_Ksiazka_Id equals ksiazka.Pk_Ksiazka_Id
                                  join czytelnik1 in dc.Czytelnik on wypozyczenie.Fk_Czytelnik_Id equals czytelnik1.Pk_Czytelnik_Id
                                  join autor in dc.Autor on ksiazka.Fk_Author_Id equals autor.Pk_Autor_Id
                                  where wypozyczenie.Fk_Czytelnik_Id == index_users
                                  select new { wypozyczenie.Pk_Wypozyczenie_Id, ksiazka.Tytul, autor.Imie, autor.Nazwisko, wypozyczenie.Data_oddania };

            if (dc.DatabaseExists()) Return1_Book.ItemsSource = books_to_return;

        }

        //Pobranie informacji o indeksie naciśnietej ksiażki w liście ksiażek
        private void List_Book_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            index_books = List_Book.SelectedIndex + 1; //informacja o naciśniętym indexie w tabeli Listy Książek
            if (dc.Ksiazka.SingleOrDefault(x => x.Pk_Ksiazka_Id == index_books) == null)
                return;
            
            Ksiazka ksiazka = dc.Ksiazka.SingleOrDefault(x => x.Pk_Ksiazka_Id == index_books);

            Borrow_Book.Visibility = Visibility.Visible; //wyświetlenie okna do wypożyczenia

            //Sprawdzić dostepność książki
            

            string Bookstatus = ksiazka.Dostepnosc;//Bookstatus przyjmuje wartość statusu ksiazki
            if (Bookstatus == "dostepna")
            {
                Bookavailable.Visibility = Visibility.Visible;
            }
            else if(Bookstatus == "wypozyczona")
            {
                Booknotavailable.Visibility = Visibility.Visible;
            }

    
        }

        //Pobranie informacji o indeksie naciśnietej ksiażki w zwrotach
        private void Return1_Book_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            index_return = Return1_Book.SelectedIndex + 1; //informacja o naciśniętym indexie w tabeli Zwrotów

            

            Return_Book.Visibility = Visibility.Visible; //wyświetlenie okna do zwrotu
        }
    }

    // wyświetlanie listy urzytkowników DataGrid Name="List_Users" (linijka 112)
    // wyświetlanie listy ksiazek DataGrid Name="List_Book" (linijka 146)
    // wyświetlanie listy wypożyczonych ksiazek przez użytkownika DataGrid Name="Rturn_Book" (linijka 177)
    // wyświetlanie listy książek a opcja dodoawania i zmieniania DataGrid Name="Add_Book" (linijka 199)
}
