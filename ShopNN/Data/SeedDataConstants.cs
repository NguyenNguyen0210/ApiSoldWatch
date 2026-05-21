using System;

namespace ShopNN.Data
{
    public static class SeedDataConstants
    {
        public static readonly Guid AdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid UserRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid AdminUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        // Category IDs
        public const int CatLuxuryId = 1;
        public const int CatSportId = 2;
        public const int CatSmartId = 3;
        public const int CatClassicId = 4;
        public const int CatFashionId = 5;
        public const int CatDiverId = 6;
        public const int CatPilotId = 7;
    }
}
