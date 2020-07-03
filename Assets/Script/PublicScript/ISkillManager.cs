namespace Assets.Script.PublicScript
{
    public interface ISkillManager
    {
        Skill.Skill GetCurSkill();
        void SetSkill(Skill.Skill skill);
    }
}

