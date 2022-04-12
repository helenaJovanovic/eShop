import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.css']
})
export class FooterComponent implements OnInit {


  public year: number;

  constructor() {
    this.year = 0;
   }

  ngOnInit(): void {
    this.year = new Date().getFullYear();
    console.log(this.year);
  }

}
