using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;

public class Client
{
    static void Main()
    {
        Console.WriteLine("Welcome to Ticketing System");
        bool LoggedIn = false;
        string ConnectionString = File.ReadAllText(@"C:\Users\User\code\EthanP1\EthanP1\ConnectionString\SQLDataBaseP1.txt");

        SqlRepository repo = new SqlRepository(ConnectionString);
        User currentUser = new User();

        while (!LoggedIn)
        {
            //Query User to Login or Register
            Console.WriteLine("Please type 'login' or 'register'");
            string response = "" + Console.ReadLine();
            Console.WriteLine("Enter your Email address: ");
            string email = "" + Console.ReadLine();
            Console.WriteLine("Enter your Password: ");
            string password = "" + Console.ReadLine();

            if (response.ToLower().Equals("login"))
            {
                if (repo.Login(email, password))
                {
                    currentUser = repo.UserLogin(email, password);
                    Console.WriteLine("Logged In Successfully");
                    LoggedIn = true;
                }
                else //else do nothing
                {
                    Console.WriteLine("Login Failed");
                }
            }
            else if (response.ToLower().Equals("register"))
            {
                if (!repo.EmailInRepo(email))
                {
                    repo.Register(email, password);
                    Console.WriteLine("Registration Successful");
                    Console.WriteLine("Logging In...");
                    currentUser = new User(email, password);
                    LoggedIn = true;
                }
                else
                {
                    Console.WriteLine("Email Unavailable");
                }
            }

        }
        while (LoggedIn)
        {
            //if manager...
            while (currentUser.GetIsManager())
            {
                //Ask what manager action to take
                Console.WriteLine("1-view pending tickets");
                Console.WriteLine("2-approve or deny pending tickets");
                string response = "" + Console.ReadLine();
                bool isViewing = false;
                bool isProcessing = false;
                //if pendingTickets...
                if (response.ToLower().Equals("1"))
                {
                    isViewing = true;
                }
                else if (response.ToLower().Equals("2"))
                {
                    isProcessing = true;
                }
                else
                {
                    Console.WriteLine("INVALID RESPONSE");
                }
                while (isViewing)
                {
                    IEnumerable<string> tix = repo.GetPendingTickets();
                    foreach (string s in tix)
                    {
                        Console.WriteLine(s);
                    }
                    Console.WriteLine("Would you like to 1-process tickets or 2-logout");
                    string cont = "" + Console.ReadLine();
                    if (cont.Equals("1"))
                    {
                        isViewing = false;
                        isProcessing = true;
                    }
                    else
                    {
                        isViewing = false;
                        LoggedIn = false;
                    }
                }
                //while processing...
                string pro = "";
                while (isProcessing)
                {
                    pro = "";
                    string ad;
                    string nextTicket = repo.ReviewTix();
                    Console.WriteLine(nextTicket);
                    Console.WriteLine("1-Approve or 2-Deny?");
                    ad = "" + Console.ReadLine();
                    if (ad.ToLower().Equals("1"))
                    {
                        repo.ChangeTicketStatus(1);
                        Console.WriteLine("Ticket Approved");
                        Console.WriteLine("1-continue, 2-view pending, 3-logout");
                        pro = "" + Console.ReadLine();
                        if (pro.ToLower().Equals("1"))
                        {
                            isProcessing = true;
                            isViewing = false;
                        }
                        else if (pro.ToLower().Equals("2"))
                        {
                            isProcessing = false;
                            isViewing = true;
                        }
                        else
                        {
                            LoggedIn = false;
                        }
                    }
                    else if (ad.ToLower().Equals("2"))
                    {
                        repo.ChangeTicketStatus(0);
                        Console.WriteLine("Ticket Denied");
                        Console.WriteLine("1-continue, 2-view pending, 3-logout");
                        pro = "" + Console.ReadLine();
                        if (pro.ToLower().Equals("1"))
                        {
                            isProcessing = true;
                            isViewing = false;
                        }
                        else if (pro.ToLower().Equals("2"))
                        {
                            isProcessing = false;
                            isViewing = true;
                        }
                        else
                        {
                            LoggedIn = false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("INVALID RESPONSE");
                    }
                }
            }
            //if not manager...
            while (!currentUser.GetIsManager())
            {
                string pro = "";
                Console.WriteLine("Would you like to: ");
                Console.WriteLine("1-view previous tickets");
                Console.WriteLine("2-create a new ticket");
                pro = "" + Console.ReadLine();
                while (pro.ToLower().Equals("1"))
                {
                    Console.WriteLine("Would you like to view:");
                    Console.WriteLine("1-approved");
                    Console.WriteLine("2-denied");
                    string s = "" + Console.ReadLine();

                    if (s.Equals("1"))
                    {
                        IEnumerable<string> tix = repo.GetUserTickets(currentUser, true);
                        foreach (string t in tix)
                        {
                            Console.WriteLine(t);
                        }
                        string g;
                        Console.WriteLine("1-create new ticket");
                        Console.WriteLine("2-view tickets");
                        Console.WriteLine("3-logout");
                        g = "" + Console.ReadLine();
                        if (g.Equals("1"))
                        {
                            pro = "2";
                        }
                        else if (g.Equals("2"))
                        {
                            pro = "1";
                        } else
                        {
                            LoggedIn = false;
                        }
                    }
                    else if (s.Equals("2"))
                    {
                        IEnumerable<string> tix = repo.GetUserTickets(currentUser, false);
                        foreach (string t in tix)
                        {
                            Console.WriteLine(t);
                        }
                        string g;
                        Console.WriteLine("1-create new ticket");
                        Console.WriteLine("2-view tickets");
                        Console.WriteLine("3-logout");
                        g = "" + Console.ReadLine();
                        if (g.Equals("1"))
                        {
                            pro = "2";
                        }
                        else if (g.Equals("2"))
                        {
                            pro = "1";
                        }
                        else
                        {
                            LoggedIn = false;
                        }
                    }
                }
                while (pro.Equals("2"))
                {
                    string desc = "";
                    double amt = 0.00;
                    string s = "";
                    Console.WriteLine("Enter a description:");
                    desc = desc + Console.ReadLine();
                    Console.WriteLine("Enter an amount in USD (two decimals)");
                    amt = amt + Double.Parse("" + Console.ReadLine());

                    repo.CreateTicket(currentUser, desc, amt);
                    Console.WriteLine("Ticket created successfully: | " + desc + " | $" + amt.ToString() + " | PENDING |");
                    Console.WriteLine("0-create new ticket");
                    Console.WriteLine("1-view previous tickets");
                    Console.WriteLine("2-logout");
                    s = s + Console.ReadLine();
                    if (s.Equals("0"))
                    {
                        pro = "2";
                    }
                    else if (s.Equals("1"))
                    {
                        pro = "1";
                    } 
                    else
                    {
                        LoggedIn = false;
                    }

                }
            }
        }
    }
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