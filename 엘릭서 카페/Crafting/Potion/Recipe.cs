using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//조합 레시피 클래스
public class Recipe
{
    public List<string> ingredientNames = new List<string>();  //필요한 재료들의 이름 목록
    public GameObject resultPotionPrefab; //조합 성공 시 생성되는 포션 프리팹
    public PotionType potionType;

    //재료들과 레시피가 일치한지 확인하는메서드
    public bool Matches(List<Ingredient> ingredients)
    {
        if (ingredients.Count != ingredientNames.Count) return false;

        for (int i =0; i < ingredientNames.Count; i++)
        {
            if (ingredients[i].ingredientName != ingredientNames[i]) return false;
        }
        return true;
    }
}

public enum PotionType
{
    BOIL,
    COLD,
    MIX
}
