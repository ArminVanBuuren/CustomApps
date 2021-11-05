using System.IO;

namespace TeleSharp.TL
{
	[TLObject(481674261)]
    public class TLVector : TLObject
    {
        public override int Constructor
        {
            get
            {
                return 481674261;
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
