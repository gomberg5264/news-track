import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { RouterTestingModule, SpyNgModuleFactoryLoader } from '@angular/router/testing';
import { ActivatedRoute, Params } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { Observable } from 'rxjs/Observable';

import { DraftEntryComponent } from './draft-entry.component';
import { httpLoaderFactory } from '../../app.module';
import { BackendApiService } from '../../services/backend-api.service';
import { DraftComponent } from '../draft/draft.component';
import { DraftFooterComponent } from '../draft-footer/draft-footer.component';
import { StorageService } from '../../services/storage.service';
import { AppSettingsService } from '../../services/app-settings.service';
import { TestBedHelper } from '../../testing/testbed.helper';
import { DataBuilder } from '../../testing/data.builder';

describe('DraftEntryComponent', () => {
  let component: DraftEntryComponent;
  let fixture: ComponentFixture<DraftEntryComponent>;
  let getDraftMock: any;
  let setVisitMock: any;
  let getDraftRelationshipMock: any;
  let getDraftSuggestionsMock: any;

  const draft = DataBuilder.getDraftsDto()[0];
  const relatedDrafts = DataBuilder.getDraftDigestsDto().splice(0, 1);
  const suggestion = DataBuilder.getDraftSuggestionDto();
  const expressions = DataBuilder.getExpressions();
  const apiServiceMock = <BackendApiService>{
    getDraft: (id) => new Observable(observer => observer.next(draft)),
    setVisit: (id) => new Observable(observer => observer.complete),
    getDraftRelationship: (id) => new Observable(observer => observer.next(relatedDrafts)),
    getDraftSuggestions: (id, take) => new Observable(observer => observer.next(suggestion))
  };

  const storageService = <StorageService>{
    getItem: (key) => 'existing'
  };

  const settingsService = <AppSettingsService>{
    getExpressions: () => new Observable<string[]>(observer => observer.next(expressions))
  };

  beforeEach(async(() => {
    getDraftMock = spyOn(apiServiceMock, 'getDraft').and.callThrough();
    setVisitMock = spyOn(apiServiceMock, 'setVisit').and.callThrough();
    getDraftRelationshipMock = spyOn(apiServiceMock, 'getDraftRelationship').and.callThrough();
    getDraftSuggestionsMock = spyOn(apiServiceMock, 'getDraftSuggestions').and.callThrough();

    TestBed.configureTestingModule({
      declarations: [
        DraftEntryComponent,
        DraftComponent,
        DraftFooterComponent
      ],
      imports: [
        HttpClientTestingModule,
        NgbModule.forRoot(),
        TranslateModule.forRoot({
          loader: {
              provide: TranslateLoader,
              useFactory: httpLoaderFactory,
              deps: [HttpClient]
          }
        }),
        RouterTestingModule
      ],
      providers: [
        { provide: BackendApiService, useFactory: () => apiServiceMock },
        { provide: StorageService, useFactory: () => storageService },
        { provide: AppSettingsService, useFactory: () => settingsService },
        { provide: ActivatedRoute, useValue: {
          params: {
            subscribe: (fn: (value: Params) => void) => fn({
                id: draft.id,
            }),
          }
        }}
      ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DraftEntryComponent);
    component = fixture.componentInstance;
    TestBedHelper.setLanguage();
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize values for the requested draft', () => {
    expect(getDraftMock).toHaveBeenCalled();
    expect(setVisitMock).toHaveBeenCalled();
    expect(getDraftRelationshipMock).toHaveBeenCalled();
    expect(getDraftSuggestionsMock).toHaveBeenCalled();
  });
});
