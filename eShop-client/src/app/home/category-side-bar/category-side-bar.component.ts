import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-category-side-bar',
  templateUrl: './category-side-bar.component.html',
  styleUrls: ['./category-side-bar.component.css']
})
export class CategorySideBarComponent implements OnInit {

  public sliderValueMin: Number;
  public sliderValueMax: Number;

  constructor() { 
    this.sliderValueMin = 0;
    this.sliderValueMax = 5000;
  }

  ngOnInit(): void {
  }

}
