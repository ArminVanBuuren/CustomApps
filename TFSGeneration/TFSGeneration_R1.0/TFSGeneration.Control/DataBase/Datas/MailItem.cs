using System;

namespace TFSAssist.Control.DataBase.Datas
{
	public struct MailItem
	{
		public string ID;
		public DateTime ReceivedDate;
		public string From;
		public string[] Recipients;
		public string Subject;
		public string Body;

        public override bool Equals(object ob)
        {
            if (ob is MailItem input)
            {
                return input.ReceivedDate == ReceivedDate && input.From == From && input.Subject == Subject && input.Body == Body;
            }

            return false;
        }

        //public override int GetHashCode()
        //{
        //    return ID.GetHashCode() ^ From.GetHashCode();
        //}
    }
}
