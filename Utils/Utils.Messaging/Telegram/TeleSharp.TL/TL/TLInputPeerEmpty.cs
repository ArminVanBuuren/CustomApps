using System.IO;

namespace TeleSharp.TL
{
    [TLObject(2134579434)]
    public class TLInputPeerEmpty : TLAbsInputPeer
    {
        public override int Constructor
        {
            get
            {
                return 2134579434;
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
