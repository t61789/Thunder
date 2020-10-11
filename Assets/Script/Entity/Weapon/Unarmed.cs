namespace Thunder.Entity.Weapon
{
    public class Unarmed : BaseWeapon
    {
        protected override void PlayerSquat(bool squatting, bool hanging)
        {
        }

        protected override void PlayerHanging(bool squatting, bool hanging)
        {
        }

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