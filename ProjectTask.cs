using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace app_test
{

    public enum PriorityList
    {
        Low,
        Medium,
        High
    }
    public class ProjectTask : INotifyPropertyChanged
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Content { get; set; }
        public PriorityList Priority { get; set; }
        private double time { get; set; }
        private bool isDone { get; set; }
        private DateTime DeadLine { get; set; }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnPropertyChanged(nameof(IsRunning));
                }
            }
        }
        private TimeSpan _remainingTime;
        public TimeSpan RemainingTime
        {
            get => _remainingTime;
            set
            {
                if (_remainingTime != value)
                {
                    _remainingTime = value;
                    Debug.WriteLine($"[RemainingTime] Nouvelle valeur: {_remainingTime} pour '{Content}'");
                    OnPropertyChanged(nameof(RemainingTime));
                    OnPropertyChanged(nameof(RemainingTimeDisplay));
                }
            }
        }
        public string RemainingTimeDisplay => RemainingTime.ToString(@"hh\:mm\:ss");

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            Debug.WriteLine($"[PropertyChanged] Notification envoyée: {name} - Abonnés: {PropertyChanged?.GetInvocationList().Length ?? 0}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ProjectTask(string content, PriorityList priority, DateTime deadLine)
        {
            Content = content;
            Priority = priority;
            DeadLine = deadLine;
            isDone = false;
            time = 0;
            RemainingTime = TimeSpan.Zero;
        }

        public ProjectTask(string content, PriorityList priority, double time, DateTime deadLine)
        {
            Content = content;
            Priority = priority;
            this.time = time;
            DeadLine = deadLine;
            isDone = false;
            RemainingTime = TimeSpan.FromHours(time);
        }

        public ProjectTask()
        {
            Content = "Task";
            Priority = PriorityList.Low;
            time = 0;
            DeadLine = DateTime.Today;
            isDone = false;
            RemainingTime = TimeSpan.Zero;
        }

        public string TimeLeft()
        {
            TimeSpan TimeLeft = this.DeadLine - DateTime.Now;
            return TimeLeft.Days.ToString();
        }
    }
}