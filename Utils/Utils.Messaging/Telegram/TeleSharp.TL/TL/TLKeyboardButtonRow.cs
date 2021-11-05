using System.IO;

namespace TeleSharp.TL
{
    [TLObject(2002815875)]
    public class TLKeyboardButtonRow : TLObject
    {
        public override int Constructor
        {
            get
            {
                return 2002815875;
            }
        }

        public TLVector<TLAbsKeyboardButton> Buttons { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            Buttons = (TLVector<TLAbsKeyboardButton>)ObjectUtils.DeserializeVector<TLAbsKeyboardButton>(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            ObjectUtils.SerializeObject(Buttons, bw);

        }
    }
}
