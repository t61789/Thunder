namespace Thunder.Entity.Weapon
{
    public class Unarmed : BaseWeapon
    {
        public override float OverHeatFactor => 0;

        public override void Fire()
        {
        }

        public override void Reload()
        {
        }

        public override void TakeOut()
        {
        }

        public override void PutBack()
        {
        }

        public override object Drop()
        {
            return null;
        }

        public override void ReadAdditionalData(object add)
        {
            
        }
    }
}