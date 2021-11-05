using System.IO;

namespace TeleSharp.TL
{
    [TLObject(-1879401953)]
    public class TLPageBlockSubtitle : TLAbsPageBlock
    {
        public override int Constructor
        {
            get
            {
                return -1879401953;
            }
        }

        public TLAbsRichText Text { get; set; }


        public void ComputeFlags()
        {

        }

        public override void DeserializeBody(BinaryReader br)
        {
            Text = (TLAbsRichText)ObjectUtils.DeserializeObject(br);

        }

        public override void SerializeBody(BinaryWriter bw)
        {
            bw.Write(Constructor);
            ObjectUtils.SerializeObject(Text, bw);

        }
    }
}