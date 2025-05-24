using System;

namespace Kassa.Models
{
    public class TableReservation
    {
        public int Id { get; set; }
        public int HallId { get; set; }
        public int TableNumber { get; set; }
        public DateTime Date { get; set; } // День резервирования (только дата, без времени)
        public TimeOnly FromTime { get; set; } // Время начала
        public TimeOnly ToTime { get; set; }   // Время конца
        public string Name { get; set; } = string.Empty; // Имя человека, на которого резерв
    }
}
