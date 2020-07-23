public struct CardInfo
{
    public CardInfo(string cardName_arg, string cardId_arg)
    {
        cardName = cardName_arg;
        cardId = cardId_arg;
        card = null;
    }

    public string cardName { get; }
    public string cardId { get; }
    public Card card;
};