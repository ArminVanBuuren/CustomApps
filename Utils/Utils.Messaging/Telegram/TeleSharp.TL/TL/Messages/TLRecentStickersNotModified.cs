using System.IO;

namespace TeleSharp.TL.Messages
{
    [TLObject(186120336)]
    public class TLRecentStickersNotModified : TLAbsRecentStickers
    {
        public override int Constructor
        {
            get
            {
                return 186120336;
            }
        }



        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);

        }
    }
}
