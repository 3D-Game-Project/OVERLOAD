public interface IAttackPart : IPart
{
    void HandleAttack(bool isAttackPressed);
    void HandleReload(bool isReloadPressed);
}