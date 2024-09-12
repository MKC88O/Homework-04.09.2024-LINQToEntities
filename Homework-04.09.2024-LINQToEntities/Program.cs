using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Homework_04._09._2024_LINQToEntities
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (var db = new ApplicationContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var event1 = new Event { Name = "Tech Conference", Date = DateTime.Now.AddMonths(1) };
                var event2 = new Event { Name = "Music Festival", Date = DateTime.Now.AddMonths(2) };

                var guest1 = new Guest { FirstName = "John", LastName = "Doe" };
                var guest2 = new Guest { FirstName = "Jane", LastName = "Smith" };

                db.Events.AddRange(event1, event2);
                db.Guests.AddRange(guest1, guest2);
                db.SaveChanges();

                //AddGuestToEvent(db, guest1.GuestId, event1.EventId, "Speaker");
                //AddGuestToEvent(db, guest2.GuestId, event1.EventId, "Participant");

                //GetGuestsForEvent(db, event1.EventId);

                //ChangeGuestRole(db, guest1.GuestId, event1.EventId, "Moderator");

                //GetEventsForGuest(db, guest1.GuestId);

                RemoveGuestFromEvent(db, guest1.GuestId, event1.EventId);

                GetEventsByRole(db, guest1.GuestId, "Speaker");
            }
        }

        public static void AddGuestToEvent(ApplicationContext db, int guestId, int eventId, string role)
        {
            var guest = db.Guests.FirstOrDefault(g => g.GuestId == guestId);

            if (guest == null)
            {
                Console.WriteLine("No guest with this ID was found.");
                return;
            }

            var eventGuest = new EventGuest
            {
                GuestId = guestId,
                EventId = eventId,
                Role = role
            };

            db.EventGuests.Add(eventGuest);
            db.SaveChanges();
            Console.WriteLine("Guest " + guest.FirstName + " " + guest.LastName + " added to event with role " + role);
        }

        public static void GetGuestsForEvent(ApplicationContext db, int eventId)
        {
            
            var even = db.Events.FirstOrDefault(e => e.EventId == eventId);
            var eventGuests = db.EventGuests
                .Include(eg => eg.Guest)
                .Where(eg => eg.EventId == eventId)
                .ToList();

            Console.WriteLine("Guests at the event " + even.Name + ":");
            foreach (var eg in eventGuests)
            {
                Console.WriteLine(eg.Guest.FirstName + eg.Guest.LastName + " - Role: " + eg.Role);
            }
        }

        public static void ChangeGuestRole(ApplicationContext db, int guestId, int eventId, string newRole)
        {
            var guest = db.Guests.FirstOrDefault(g => g.GuestId == guestId);
            var eventGuest = db.EventGuests.FirstOrDefault(eg => eg.GuestId == guestId && eg.EventId == eventId);
            if (eventGuest != null)
            {
                eventGuest.Role = newRole;
                db.SaveChanges();
                Console.WriteLine("Role " + guest.FirstName + " " + guest.LastName + " changed to " + newRole);
            }
        }

        public static void GetEventsForGuest(ApplicationContext db, int guestId)
        {
            var guest = db.Guests.FirstOrDefault(g => g.GuestId == guestId);

            if (guest == null)
            {
                Console.WriteLine("No guest with this ID was found.");
                return; 
            }

            var events = db.EventGuests
                .Include(eg => eg.Event)
                .Where(eg => eg.GuestId == guestId)
                .Select(eg => eg.Event)
                .ToList();

            Console.WriteLine("Events for the guest " + guest.FirstName + guest.LastName + ":");

            if (events.Count == 0)
            {
                Console.WriteLine("There are no events for this guest.");
            }
            else
            {
                foreach (var ev in events)
                {
                    Console.WriteLine(ev.Name + " - Date: " + ev.Date);
                }
            }
        }

        public static void RemoveGuestFromEvent(ApplicationContext db, int guestId, int eventId)
        {
            var guest = db.Guests.FirstOrDefault(g => g.GuestId == guestId);
            var eventGuest = db.EventGuests.FirstOrDefault(eg => eg.GuestId == guestId && eg.EventId == eventId);
            if (eventGuest != null)
            {
                db.EventGuests.Remove(eventGuest);
                db.SaveChanges();
                Console.WriteLine("Guest " + guest.FirstName + " " + guest.LastName + " removed from event");
            }
        }

        public static void GetEventsByRole(ApplicationContext db, int guestId, string role)
        {
            var guest = db.Guests.FirstOrDefault(g => g.GuestId == guestId);

            if (guest == null)
            {
                Console.WriteLine("No guest with this ID was found.");
                return;
            }

            var events = db.EventGuests
                .Include(eg => eg.Event)
                .Where(eg => eg.GuestId == guestId && eg.Role == role)
                .Select(eg => eg.Event)
                .ToList();

            Console.WriteLine("Events for the guest " + guest.FirstName + " " + guest.LastName + " in the role" + role + ":");
            foreach (var ev in events)
            {
                Console.WriteLine("\t" + ev.Name + "- Date: " + ev.Date);
            }
        }
    }


    public class Event
    {
        public int EventId { get; set; }
        public string? Name { get; set; }
        public DateTime Date { get; set; }
        public List<EventGuest> EventGuests { get; set; } = new List<EventGuest>();
    }

    public class Guest
    {
        public int GuestId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public List<EventGuest> EventGuests { get; set; } = new List<EventGuest>();
    }

    public class EventGuest
    {
        public int EventId { get; set; }
        public Event Event { get; set; }
        public int GuestId { get; set; }
        public Guest Guest { get; set; }
        public string Role { get; set; }
    }


    public class ApplicationContext : DbContext
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<EventGuest> EventGuests { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.LogTo(e => Console.WriteLine(e), new[] { RelationalEventId.CommandExecuted });
            optionsBuilder.UseSqlServer(@"Server=DESKTOP-M496S5I;Database=LINQ;Trusted_Connection=True; TrustServerCertificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventGuest>()
                .HasKey(eg => new { eg.EventId, eg.GuestId });

            modelBuilder.Entity<EventGuest>()
                .HasOne(eg => eg.Event)
                .WithMany(e => e.EventGuests)
                .HasForeignKey(eg => eg.EventId);

            modelBuilder.Entity<EventGuest>()
                .HasOne(eg => eg.Guest)
                .WithMany(g => g.EventGuests)
                .HasForeignKey(eg => eg.GuestId);
        }
    }
}
