using CivClone.Simulation;
using UnityEngine;

namespace CivClone.Presentation
{
    public sealed class CityHover : MonoBehaviour
    {
        private CityPresenter presenter;
        private City city;

        public City City => city;

        public void Bind(CityPresenter presenterRef, City cityRef)
        {
            presenter = presenterRef;
            city = cityRef;
        }

        private void OnMouseEnter()
        {
            presenter?.ShowCityHover(city);
        }

        private void OnMouseExit()
        {
            presenter?.ClearCityHover(city);
        }
    }
}
