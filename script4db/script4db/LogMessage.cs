using System;
using System.Drawing;

namespace script4db
{
    public enum LogMessageTypes
    {
        Info,
        Warning,
        Error
    }

    class LogMessage
    {
        private LogMessageTypes type;
        private String text;
        private String source;
        private Color color;
        private Color[] ColorByType = new Color[] { Color.DarkGreen, Color.DarkOrange, Color.Red };
        private int typeNameMaxWidth;
        private String typeNameNormalized;

        public LogMessage(LogMessageTypes msgType, String msgSource, String msgText)
        {
            this.type = msgType;
            this.source = msgSource;
            this.text = msgText;
            this.color = ColorByType[(int)msgType];
        }

        public LogMessageTypes Type
        {
            get
            {
                return type;
            }
        }

        public string Source
        {
            get
            {
                return source;
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
        }

        public Color Color
        {
            get
            {
                return color;
            }
        }

        public string TypeNameNormalized
        {
            get
            {
                if (typeNameNormalized == null)
                {
                    typeNameNormalized = this.type.ToString().PadRight(TypeNameMaxWidth);
                }
                return typeNameNormalized;
            }
        }

        public int TypeNameMaxWidth
        {
            get
            {
                if (typeNameMaxWidth == 0)
                {
                    foreach (LogMessageTypes enumVal in Enum.GetValues(typeof(LogMessageTypes)))
                    {
                        this.typeNameMaxWidth = Math.Max(this.typeNameMaxWidth, enumVal.ToString().Length);
                    }
                }
                return typeNameMaxWidth;
            }

        }
    }
}
