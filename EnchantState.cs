/*
 * 작성자: 윤정도
 * 생성일: 6/11/2023 7:46:39 PM
 *
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MapleJaeMul
{
    public enum MVPClass
    {
        Bronze,
        Silver,
        Gold,
        Diamond,
        Red
    }

    public enum ItemLevel
    {
        _140,
        _150,
        _160,
        _200,
        _250
    }

    public class ItemInfo
    {
        public string Name { get; }
        public ItemLevel Level { get; }
        public BitmapImage Source { get; }  
        public ItemInfo(string name, ItemLevel level, BitmapImage image)
        {
            Name = name;
            Level = level;
            Source = image;
        }

        public const string ResourcePath = "pack://application:,,,/MapleJaeMul;component/item/";
        public static readonly ItemInfo 도미네이터_펜던트 = new("도미네이터 펜던트", ItemLevel._140, new BitmapImage(new Uri(ResourcePath + "140.png")));
        public static readonly ItemInfo 하이네스_워리어_헬름 = new("하이네스 워리어 헬름", ItemLevel._150, new BitmapImage(new Uri(ResourcePath + "150.png")));
        public static readonly ItemInfo 루즈_컨트롤_머신_마크 = new("루즈_컨트롤_머신_마크", ItemLevel._160, new BitmapImage(new Uri(ResourcePath + "160.png")));
        public static readonly ItemInfo 아케인셰이드_아처숄더 = new("아케인셰이드 아처숄더", ItemLevel._200, new BitmapImage(new Uri(ResourcePath + "200.png")));
        public static readonly ItemInfo 에테르넬_메이지로브 = new("에테르넬 메이지로브", ItemLevel._250, new BitmapImage(new Uri(ResourcePath + "250.png")));
    }

    public class EnchantState : INotifyPropertyChanged
    {
        private ItemInfo _item = ItemInfo.도미네이터_펜던트;

        public int Star { get; set; } = 0;

        public ItemInfo Item
        {
            get => _item;
            set
            {
                _item = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemName));
                OnPropertyChanged(nameof(ItemLevel));
                OnPropertyChanged(nameof(ItemImageSource));
            }
        }

        public string ItemName => Item.Name;         // 아이템 명
        public ItemLevel ItemLevel => Item.Level;
        public ImageSource ItemImageSource => Item.Source;
        public int ContinousDecreaseCount { get; set; } = 0;
        public bool SuperCatch => ContinousDecreaseCount >= 2;      // 연속 2번 하락실패시 슈퍼캣치
        public bool UseStarCatch { get; set; } = false;
        public bool PreventDestroy { get; set; } = false;

        public int AllStat => s_AllStat[ItemLevel][Star];
        public int WeaponPower => s_WeaponPower[ItemLevel][Star];
        public int EnchangeOriginalMeso => s_EnchantMeso[ItemLevel][Star];
        public int EnchangeMeso
        {
            get
            {
                int enchantMeso = (int)(EnchangeOriginalMeso * (1.0f - TotalDiscount));

                if (Star >= 15 && Star <= 16 && PreventDestroy)
                    enchantMeso += EnchangeOriginalMeso;

                return enchantMeso;
            }
        }

        public float SuccessProb => SuperCatch ? 100 : s_SuccessProb[Star];
        public bool SundayMaple { get; set; } = false;
        public float SundayMapleDiscount => SundayMaple ? 0.3f : 0.0f;                  // 썬데이 메이플 30퍼 할인
        public bool PremiumPC { get; set; } = false;
        public float PremiumPCDiscount => PremiumPC ? 0.05f : 0.0f;                     // PC방 5퍼 할인
        public MVPClass MVP { get; set; } = MVPClass.Bronze;                            // MVP 할인

        public float MVPDiscount
        {
            get
            {
                if (MVP == MVPClass.Silver) return 0.03f;
                if (MVP == MVPClass.Gold) return 0.05f;
                if (MVP == MVPClass.Diamond || MVP == MVPClass.Red) return 0.1f;
                return 0.0f;
            }
        }

        // 썬데이메이플 곱적용
        // MVP, 프리미엄PC 합적용
        // 최고로 할인받으면 기존 비용의 ((1 - 0.1 - 0.05) × 0.7) = 0.595배 비용으로 40.5% 할인된 금액으로 강화가능
        public float TotalDiscount => 1.0f - ((1.0f - PremiumPCDiscount - MVPDiscount) * (1 - SundayMapleDiscount));
        public float FailProb {
            get
            {
                if (SuperCatch) return 0.0f;
                if (PreventDestroy) return 100.0f - SuccessProb;
                return 100.0f - SuccessProb - s_DestroyProb[Star];
            }
        }

        public float DestroyProb
        {
            get
            {
                if (SuperCatch) return 0.0f;
                if (PreventDestroy) return 0.0f;
                return s_DestroyProb[Star];
            }
        }

        public string OptionText()
        {
            StringBuilder builder = new(2048);

            builder.AppendLine($"{Star} > {Star + 1}");
            builder.AppendLine($"성공확률 : {SuccessProb}");

            if (FailProb > 0)
            {
                if (Star <= 15 || Star == 20) builder.AppendLine($"실패(유지)확률 : {FailProb}%");
                else builder.AppendLine($"실패(하락)확률 : {FailProb}%");
            }

            if (DestroyProb > 0)
            {
                builder.AppendLine($"파괴확률 : {DestroyProb}%");
            }

            builder.AppendLine();

            int allStat = AllStat;
            int weaponPower = WeaponPower;

            if (allStat > 0)
            {
                builder.AppendLine($"STR : +{allStat}");
                builder.AppendLine($"DEX : +{allStat}");
                builder.AppendLine($"INT : +{allStat}");
                builder.AppendLine($"LUK : +{allStat}");
            }

            if (weaponPower > 0)
            {
                builder.AppendLine($"공격력/마력 : +{weaponPower}");
            }
            

            return builder.ToString();
        }

        public static readonly Dictionary<ItemLevel, int[]> s_AllStat = new()
        {
            {
                ItemLevel._140,
                new []
                {
                    2, 2, 2, 2, 2,
                    3, 3, 3, 3, 3,
                    3, 3, 3, 3, 3,
                    9, 9, 9, 9, 9,
                    9, 9, 0, 0, 0
                }
            },
            {
                ItemLevel._150,
                new []
                {
                    2, 2, 2, 2, 2,
                    3, 3, 3, 3, 3,
                    3, 3, 3, 3, 3,
                    11, 11, 11, 11, 11,
                    11, 11, 0, 0, 0
                }
            },
            {
                ItemLevel._160,
                new []
                {
                    2, 2, 2, 2, 2,
                    3, 3, 3, 3, 3,
                    3, 3, 3, 3, 3,
                    13, 13, 13, 13, 13,
                    13, 13, 0, 0, 0
                }
            },
            {
                ItemLevel._200,
                new []
                {
                    2, 2, 2, 2, 2,
                    3, 3, 3, 3, 3,
                    3, 3, 3, 3, 3,
                    15, 15, 15, 15, 15,
                    15, 15, 0, 0, 0
                }
            },
            {
                ItemLevel._250,
                new []
                {
                    2, 2, 2, 2, 2,
                    3, 3, 3, 3, 3,
                    3, 3, 3, 3, 3,
                    17, 17, 17, 17, 17,
                    17, 17, 0, 0, 0
                }
            }

        };



        public static readonly Dictionary<ItemLevel, int[]> s_WeaponPower = new ()
        {
            {
                ItemLevel._140,
                new []
                {
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    8, 9, 10, 11, 12,
                    13, 15, 17, 19, 21
                }
            },
            {
                ItemLevel._150,
                new []
                {
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    9, 10, 11, 12, 13,
                    14, 16, 18, 20, 22
                }
            },
            {
                ItemLevel._160,
                new []
                {
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    10, 11, 12, 13, 14,
                    15, 17, 19, 21, 23
                }
            },
            {
                ItemLevel._200,
                new []
                {
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    12, 13, 14, 15, 16,
                    17, 19, 21, 23, 25
                }
            },
            {
                ItemLevel._250,
                new []
                {
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    14, 15, 16, 17, 18,
                    19, 21, 23, 25, 27
                }
            }
        };

        public static readonly float[] s_SuccessProb = new[]
        {
            95.0f, 90.0f, 85.0f, 85.0f, 80.0f,
            75.0f, 70.0f, 65.0f, 60.0f, 55.0f,
            50.0f, 45.0f, 40.0f, 35.0f, 30.0f,
            30.0f, 30.0f, 30.0f, 30.0f, 30.0f,
            30.0f, 30.0f, 3.0f, 2.0f, 1.0f,
        };

        public static readonly float[] s_DestroyProb = new[]
        {
            0, 0, 0, 0, 0,
            0, 0, 0, 0, 0,
            0, 0, 0, 0, 0,
            2.1f, 2.1f, 2.1f, 2.8f, 2.8f,
            7.0f, 7.0f, 19.4f, 29.4f, 39.6f
        };



        public static readonly Dictionary<ItemLevel, int[]> s_EnchantMeso = new()
        {
            {
                ItemLevel._140,
                new []
                {
                    110800    ,
                    220500    ,
                    330300    ,
                    440000    ,
                    549800    ,
                    659600    ,
                    769300    ,
                    879100    ,
                    988800    ,
                    1098600   ,
                    4448200   ,
                    5625900   ,
                    6982900   ,
                    8529400   ,
                    10275700  ,
                    24462200  ,
                    28812500  ,
                    33620400  ,
                    38904500  ,
                    44683300  ,
                    62696400  ,
                    71087200  ,
                    80152000  ,
                    89912300  ,
                    100389000
                }
            },
            {
                ItemLevel._150,
                new []
                {
                    136000    ,
                    271000    ,
                    406000    ,
                    541000    ,
                    676000    ,
                    811000    ,
                    946000    ,
                    1081000   ,
                    1216000   ,
                    1351000   ,
                    5470800   ,
                    6919400   ,
                    8588400   ,
                    10490600  ,
                    12638500  ,
                    30087200  ,
                    35437900  ,
                    41351400  ,
                    47850600  ,
                    54958200  ,
                    62696400  ,
                    71087200  ,
                    80152000  ,
                    89912300  ,
                    100389000 
                }
            },
            {
                ItemLevel._160,
                new []
                {
                    164800	    ,
                    328700	    ,
                    492500	    ,
                    656400	    ,
                    820200	    ,
                    984000	    ,
                    1147900	    ,
                    1311700	    ,
                    1475600	    ,
                    1639400	    ,
                    6639400	    ,
                    8397300	    ,
                    10422900	,
                    12731500	,
                    15338200	,
                    36514500	,
                    43008300	,
                    50185100	,
                    58072700	,
                    66698700	,
                    76090000	,
                    86273300	,
                    97274600	,
                    109120000	,
                    121834900
                }
            },
            {
                ItemLevel._200,
                new []
                {
                    321000	    ,
                    641000	    ,
                    961000	    ,
                    1281000	    ,
                    1601000	    ,
                    1921000	    ,
                    2241000	    ,
                    2561000	    ,
                    2881000	    ,
                    3201000	    ,
                    12966500	,
                    16400100	,
                    20356300	,
                    24865300	,
                    29956500	,
                    71316500	,
                    83999600	,
                    98016700	,
                    113422300	,
                    130270000	,
                    148612400	,
                    168501500	,
                    189988600	,
                    213124000	,
                    237957700
                }
            },
            {
                ItemLevel._250,
                new []
                {
                    626000    ,
                    1251000   ,
                    1876000   ,
                    2501000   ,
                    3126000   ,
                    3751000   ,
                    4376000   ,
                    5001000   ,
                    5626000   ,
                    6251000   ,
                    25324300  ,
                    32030400  ,
                    39757400  ,
                    48564000  ,
                    58507800  ,
                    139289100 ,
                    164060800 ,
                    191438000 ,
                    221527000 ,
                    254432600 ,
                    290257600 ,
                    329103600 ,
                    371070400 ,
                    416256900 ,
                    464760300
                }
            }
        };




        /* https://velog.io/@darkpppet/%EB%A9%94%EC%9D%B4%ED%94%8C-%EC%8A%A4%ED%83%80%ED%8F%AC%EC%8A%A4-%EB%B9%84%EC%9A%A9
       140      150	        160	        200	        250
       110800	136000	    164800	    321000	    626000
       220500	271000	    328700	    641000	    1251000
       330300	406000	    492500	    961000	    1876000
       440000	541000	    656400	    1281000	    2501000
       549800	676000	    820200	    1601000	    3126000
       659600	811000	    984000	    1921000	    3751000
       769300	946000	    1147900	    2241000	    4376000
       879100	1081000	    1311700	    2561000	    5001000
       988800	1216000	    1475600	    2881000	    5626000
       1098600	1351000	    1639400	    3201000	    6251000
       4448200	5470800	    6639400	    12966500	25324300
       5625900	6919400	    8397300	    16400100	32030400
       6982900	8588400	    10422900	20356300	39757400
       8529400	10490600	12731500	24865300	48564000
       10275700	12638500	15338200	29956500	58507800
       24462200	30087200	36514500	71316500	139289100
       28812500	35437900	43008300	83999600	164060800
       33620400	41351400	50185100	98016700	191438000
       38904500	47850600	58072700	113422300	221527000
       44683300	54958200	66698700	130270000	254432600
       50974700	62696400	76090000	148612400	290257600
       57796700	71087200	86273300	168501500	329103600
       65166700	80152000	97274600	189988600	371070400
       73102200	89912300	109120000	213124000	416256900
       81620200	100389000	121834900	237957700	464760300

        */


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
