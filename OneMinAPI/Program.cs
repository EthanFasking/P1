using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
using System.Net.Http.Formatting;
using DoneMinAPI;


    class Program
    {
        static HttpClient client = new HttpClient();

        public static void Main(string[] args)
        {   
            RunAsync().GetAwaiter().GetResult();
        }

        static async Task RunAsync()
        {
            client.BaseAddress = new Uri("https://localhost:5000/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {  
                  User currentUser = new User();
                  Ticket ticket = new Ticket();
                  List<Ticket> tickets = new List<Ticket>();
                  List<User> users = new List<User>();
                  currentUser = await login();
                  getUser(currentUser);

            } 
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }


        }

        static void getUser(User user)
        {
            Console.WriteLine($"email: {user.GetUsername()}\t " +
                $"password: {user.GetPassword()}\t " +
                $"isManager: {user.GetIsManager()}\t "
                );
        }
        
        static void displayTicket(Ticket ticket)
        {
            Console.WriteLine($"ReferenceNumber: {ticket.GetId()}\t " +
                $"Description: '{ticket.GetDescription()}'\t " +
                $"Amount: {ticket.GetAmount().ToString()}\t " + 
                $"isPending: {ticket.GetIsPending().ToString()}\t +" +
                $"isApproved: {ticket.GetIsApproved().ToString()}\t ");
        }

        static void displayTicketsList(List<Ticket> tickets)
        {
            foreach(Ticket ticket in tickets)
                displayTicket(ticket);
        }

        //takes a URL and returns the employee
        static async Task<User> GetUserAsync(string path)
        {
            User u = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                u = await response.Content.ReadAsAsync<User>();
            }
            return u;
        }

        

        //creates employee and returns a URL which is the location of employee
        static async Task<Uri> RegisterUserAsync(User u)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(
                "Users", u);
            response.EnsureSuccessStatusCode();

            return response.Headers.Location;
        }

        static async Task<User> login()
        {
            User currentUser = new User();
            Console.WriteLine("Please enter your email");
            string email = Console.ReadLine();
            Console.WriteLine("Please enter your password");
            string password = Console.ReadLine();

            HttpResponseMessage response = await client.GetAsync($"/login/{email}/{password}");
            if (response.IsSuccessStatusCode)
            {
                currentUser = await response.Content.ReadAsAsync<User>();
            }
            return currentUser;
        }
        
        //Manager methods
        }

public class Ticket
{
    //Fields
    public long Id;
    private string email;
    private string description;
    private double amount;
    private bool isPending;
    private bool isApproved;

    //Getters/Setters
    public long GetId() { return this.Id; }
    public string GetEmail() { return this.email; }
    public string GetDescription() { return this.description; }
    public double GetAmount() { return this.amount; }
    public bool GetIsPending() { return this.isPending; }
    public bool GetIsApproved() { return this.isApproved; }
    public void SetId(long e) { this.Id = e; }
    public void SetEmail(string e) { this.email = e; }
    public void SetDescription(string d) { this.description = d; }
    public void SetAmount(double a) { this.amount = a; }
    public void SetIsPending(bool p) { this.isPending = p; }
    public void SetIsApproved(bool a) { this.isApproved = a; }

    //Constructor
    public Ticket()
    {
        this.SetDescription("");
        this.SetAmount(0.00);
        this.isPending = true;
        this.isApproved = false;
    }

    //Methods
    override public string ToString()
    {
        string ticket = "";
        ticket += this.GetDescription() + " | $";
        ticket += this.GetAmount().ToString() + " | ";
        if (this.GetIsPending()) { ticket += "PENDING"; }
        else
        {
            if (this.GetIsApproved()) { ticket += "APPROVED"; }
            else { ticket += "DENIED"; }
        }

        return ticket;
    }
}

public class User
{
    //Fields
    private string email;
    private string password;
    private bool isManager;

    //Constructors

    //The default constructor should set the level to Employee
    public User()
    {
        this.isManager = false;
    }
    //The standard constructor for this class creates a User with a specific email and password,
    //and a default level of "Employee"
    public User(string e, string pw)
    {
        this.SetUsername(e);
        this.SetPassword(pw);
        this.isManager = false;
    }
    public User(string e, string pw, bool im)
    {
        this.SetUsername(e);
        this.SetPassword(pw);
        this.isManager = im;
    }

    //Getters/Setters
    public string GetUsername() { return this.email; }
    public string GetPassword() { return this.password; }
    public bool GetIsManager() { return this.isManager; }
    public void SetUsername(string e) { this.email = e; }
    public void SetPassword(string p) { this.password = p; }
    public void SetIsManager(bool im) { this.isManager = im; }

    //Methods
}

public interface IRepository
{
    public IEnumerable<string> GetPendingTickets();
    public IEnumerable<string> GetUserTickets(User u, bool s);
    public void AddTicket(Ticket t);
    public bool EmailInRepo(string s);
    public bool Login(string email, string password);
    public User UserLogin(string username, string password);
    public User Register(string email, string password);
    public void ChangeTicketStatus(int isApproved);
    public Ticket CreateTicket(User u, string desc, double amt);
    public string ReviewTix();
}

public class SqlRepository : IRepository
{
    //Fields
    private readonly string _connectionString;

    //Constructors
    public SqlRepository(string connectionString)
    {
        this._connectionString = connectionString;
    }

    //Methods

    //GetTickets gets all tickets of type Pending, Approved, or Denied
    public IEnumerable<string> GetPendingTickets()
    {
        List<string> tickets = new List<string>();
        using SqlConnection connection = new SqlConnection(this._connectionString);
        connection.Open();
        string cmdText = "SELECT [Description], [Amount], [IsApproved] FROM [Database].[Tickets] WHERE [IsPending] = 1;";

        using SqlCommand cmd = new SqlCommand(cmdText, connection);
        using SqlDataReader reader = cmd.ExecuteReader();

        string desc, amt, pending, ct;

        while (reader.Read())
        {
            desc = reader.GetString(0);
            amt = reader.GetDecimal(1).ToString();
            pending = reader.GetBoolean(2).ToString();
            ct = "Description: " + desc + " | $" + amt + " | IsApproved: " + pending;
            tickets.Add(ct);
        }

        connection.Close();
        return tickets;
    }

    //GetTickets gets all tickets of type Pending, Approved, or Denied
    public IEnumerable<string> GetUserTickets(User u, bool s)
    {
        List<string> tickets = new List<string>();
        using SqlConnection connection = new SqlConnection(this._connectionString);
        connection.Open();
        string cmdText = "SELECT [Description], [Amount], [IsPending], [IsApproved] " +
                        "FROM [Database].[Tickets] WHERE [Email] = '" + u.GetUsername() + "' AND [IsPending] = 0";
        if (s)
        {
            cmdText += " AND [IsApproved] = 1;";
        }
        else
        {
            cmdText += " AND [IsApproved] = 0;";
        }

        using SqlCommand cmd = new SqlCommand(cmdText, connection);
        using SqlDataReader reader = cmd.ExecuteReader();

        string desc, amt, pending, ct;

        while (reader.Read())
        {
            desc = reader.GetString(0);
            amt = reader.GetDecimal(1).ToString();
            pending = reader.GetBoolean(2).ToString();
            ct = desc + " | $" + amt + " | " + pending;
            tickets.Add(ct);
        }

        connection.Close();
        return tickets;
    }

    //AddTicket takes a given ticket and adds it to the Database.Tickets
    public void AddTicket(Ticket t)
    {
        //open new connection
        using SqlConnection connection = new SqlConnection(this._connectionString);
        connection.Open();

        //create command to insert provided ticket into Database.Tickets
        string cmdText = "INSERT INTO [Database].[Tickets] ([Id], [Description], [Amount], [IsPending], [IsApproved]) VALUES (" + ", "
                          + t.GetId() + ", '" + t.GetDescription() + "', " + t.GetAmount().ToString() + ", "
                          + t.GetIsPending().ToString() + ", " + t.GetIsApproved().ToString() + ");";

        using SqlCommand cmd = new SqlCommand(cmdText, connection);

        //execute command
        cmd.ExecuteNonQuery();

        //close the connection
        connection.Close();

    }

    //checks the repo for a given email
    public bool EmailInRepo(string s)
    {
        List<string> emails = new List<string>();
        string cmdText = "SELECT [Email] FROM [Database].[User]";
        using (SqlConnection connection = new SqlConnection(this._connectionString))
        {
            SqlCommand cmd = new SqlCommand(cmdText, connection);
            connection.Open();
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    emails.Add(reader.GetString(0));
                }
            }
            connection.Close();
        }

        return emails.Contains(s);
    }

    //validPassword takes an email and password and returns whether or not they match an existing account
    public bool Login(string email, string password)
    {
        if (EmailInRepo(email))
        {
            using SqlConnection connection = new SqlConnection(this._connectionString);
            connection.Open();
            string cmdText = "SELECT [Password] FROM [Database].[User] WHERE [Email] = '" + email + "';";

            using SqlCommand cmd = new SqlCommand(cmdText, connection);
            using SqlDataReader reader = cmd.ExecuteReader();

            string answer = "";
            while (reader.Read())
            {
                answer = reader.GetString(0);
            }

            connection.Close();
            bool output = answer.Equals(password);
            return output;
        }
        else
        {
            return false;
        }
    }

    public User UserLogin(string email, string password)
    {
        User currentUser = new User();
        if (this.Login(email, password))
        {
            using SqlConnection connection = new SqlConnection(this._connectionString);
            connection.Open();
            string cmdText = "SELECT [isManager] FROM [Database].[User] WHERE [Email] = '" + email + "';";

            using SqlCommand cmd = new SqlCommand(cmdText, connection);
            using SqlDataReader reader = cmd.ExecuteReader();


            while (reader.Read())
            {
                bool im = reader.GetBoolean(0);
                currentUser = new User(email, password, im);
            }

            connection.Close();
        }
        return currentUser;
    }

    //Register takes an email and a password, and creates a new user
    //method also returns a boolean indicating whether the Registration was successful
    public User Register(string email, string password)
    {
        using SqlConnection connection = new SqlConnection(this._connectionString);
        connection.Open();
        string cmdText = "INSERT INTO [Database].[User] ([Email], [Password], [isManager]) " + "VALUES ('" + email + "', '" + password + "', 0);";

        using SqlCommand cmd = new SqlCommand(cmdText, connection);

        cmd.ExecuteNonQuery();
        connection.Close();

        return new User(email, password);
    }

    public void ChangeTicketStatus(int isApproved)
    {
        using SqlConnection connection = new SqlConnection(this._connectionString);
        connection.Open();
        string cmdID = "SELECT TOP 1 [ReferenceNumber] FROM [Database].[Tickets] WHERE [IsPending] = 1;";
        using SqlCommand cmdRN = new SqlCommand(cmdID, connection);
        using SqlDataReader readerID = cmdRN.ExecuteReader();

        int refNum = 0;
        while (readerID.Read())
        {
            refNum = readerID.GetInt32(0);
        }
        connection.Close();
        connection.Open();
        //change ticket status
        string cmdad = "UPDATE [Database].[Tickets] SET [IsApproved] = " + isApproved.ToString() + ", [IsPending] = 0 WHERE [ReferenceNumber] = " + refNum.ToString() + ";";
        using SqlCommand finCmd = new SqlCommand(cmdad, connection);

        finCmd.ExecuteNonQuery();
        connection.Close();
    }

    //Create ticket takes in a string and a double and creates a ticket
    //This method also inserts the ticket into the database
    public Ticket CreateTicket(User u, string desc, double amt)
    {
        Ticket t = new Ticket();
        t.SetEmail(u.GetUsername());
        t.SetDescription(desc);
        t.SetAmount(amt);

        using SqlConnection connection = new SqlConnection(this._connectionString);
        connection.Open();
        string cmd = "INSERT INTO [Database].[Tickets] ([Email], [Description], [Amount], [IsPending], [IsApproved]) VALUES ('"
                          + t.GetEmail() + "', '" + t.GetDescription() + "', " + t.GetAmount() + ", '"
                          + t.GetIsPending().ToString() + "', '" + t.GetIsApproved().ToString() + "');";
        using SqlCommand command = new SqlCommand(cmd, connection);


        command.ExecuteNonQuery(); //Add ticket to ticket table
        connection.Close(); //Close connection

        return t;
    }

    //reviewTix operates on a Manager, allowing them to approve/deny tickets
    public string ReviewTix()
    {
        using SqlConnection connection = new SqlConnection(this._connectionString);
        connection.Open();
        string cmd1 = "SELECT TOP 1 [Description], [Amount], [IsPending] [IsApproved] FROM [Database].[Tickets] WHERE [IsPending] = 1;";
        using SqlCommand cmdID = new SqlCommand(cmd1, connection);
        using SqlDataReader readerID = cmdID.ExecuteReader();

        string desc, amt, pending, ct;
        ct = "";

        while (readerID.Read())
        {
            desc = readerID.GetString(0);
            amt = readerID.GetDecimal(1).ToString();
            pending = readerID.GetBoolean(2).ToString();
            ct = desc + " | $" + amt + " | " + pending;
        }
        return ct;
    }
}